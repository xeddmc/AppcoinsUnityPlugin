﻿using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class Startup
{
    public const string DEFAULT_UNITY_PACKAGE_IDENTIFIER = "com.Company.ProductName";

    static Startup()
    {
        //Check if the active platform is Android. If it isn't change it
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            #if UNITY_5
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);

            #else
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            #endif

        //Check if min sdk version is lower than 21. If it is, set it to 21
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel21)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;

        //Check if the bunde id is the default one and change it if it to avoid that error        
        #if UNITY_5
            if (PlayerSettings.bundleIdentifier == DEFAULT_UNITY_PACKAGE_IDENTIFIER)
                PlayerSettings.bundleIdentifier = "com.aptoide.appcoins";

        #else
            if (PlayerSettings.applicationIdentifier == DEFAULT_UNITY_PACKAGE_IDENTIFIER)
                PlayerSettings.applicationIdentifier = "com.aptoide.appcoins";
        
        #endif

        //Make sure that gradle is the selected build system
        if (EditorUserBuildSettings.androidBuildSystem != AndroidBuildSystem.Gradle)
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;      


        Debug.Log("Successfully integrated Appcoins Unity plugin!");
    }
}