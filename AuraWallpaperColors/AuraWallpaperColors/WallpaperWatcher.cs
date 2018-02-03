using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    public class WallpaperWatcher : IObservable<string>
    {
        IObservable<string> WallpaperObservable { get; }

        public WallpaperWatcher()
        {
            WallpaperObservable = Observable.Create((IObserver<string> obs) =>
            {
                return StartWatchingRegistry(obs);
            }).Publish().RefCount();
        }

        IDisposable StartWatchingRegistry(IObserver<string> observer)
        {
            IDisposable fileSubscription = SubscribeToWallpaperChanges(observer);

            var registryWatcher = GetWallpaperWatcher();

            EventArrivedEventHandler registryEventHandler = (object sender, EventArrivedEventArgs e) =>
            {
                fileSubscription?.Dispose();
                fileSubscription = SubscribeToWallpaperChanges(observer);
            };

            registryWatcher.EventArrived += registryEventHandler;
            registryWatcher.Start();

            return Disposable.Create(() =>
            {
                fileSubscription?.Dispose();
                registryWatcher.EventArrived -= registryEventHandler;
                registryWatcher.Stop();
                registryWatcher.Dispose();
            });
        }

        ManagementEventWatcher GetWallpaperWatcher()
        {
            var identity = WindowsIdentity.GetCurrent().User.Value;

            var q = "SELECT * FROM RegistryValueChangeEvent " +
                    "WHERE Hive = 'HKEY_USERS' AND KeyPath = '" + identity + @"\\Control Panel\\Desktop' " +
                    "AND ValueName='WallPaper' ";
            WqlEventQuery query = new WqlEventQuery(q);

            return new ManagementEventWatcher(query);
        }

        IDisposable SubscribeToWallpaperChanges(IObserver<string> obs)
        {
            var path = GetWallpaperPathFromRegistry();
            obs.OnNext(path);
            return GetPathChangedObservable(path).Subscribe(_ => obs.OnNext(path));
        }

        IObservable<Unit> GetPathChangedObservable(string path)
        {
            return Observable.Create((IObserver<Unit> obs) =>
            {
                var watcher = new FileSystemWatcher(Path.GetDirectoryName(path));
                FileSystemEventHandler handler = (object sender, FileSystemEventArgs e) =>
                {
                    if (e.Name == Path.GetFileName(path))
                    {
                        // wait a while so that the file is probably not in use
                        var t = new Timer((_) => obs.OnNext(Unit.Default), null, 100, Timeout.Infinite);
                    }
                };
                watcher.EnableRaisingEvents = true;
                watcher.Changed += handler;

                return Disposable.Create(() =>
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Changed -= handler;
                    watcher.Dispose();
                });
            });
        }

        string GetWallpaperPathFromRegistry()
        {
            var wpReg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", System.Security.AccessControl.RegistryRights.Notify | System.Security.AccessControl.RegistryRights.ReadKey);
            var path = wpReg.GetValue("WallPaper").ToString();
            wpReg.Close();
            return path;
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return WallpaperObservable.Subscribe(observer);
        }
    }
}
