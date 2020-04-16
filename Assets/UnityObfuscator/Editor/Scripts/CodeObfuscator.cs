﻿using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;

namespace Flower.UnityObfuscator
{
    internal static class CodeObfuscator
    {
        public static void DoObfuscate(string[] assemblyPath, string uselessCodeLibAssemblyPath, int randomSeed, bool enableNameObfuscate, bool enableCodeInject,
            ObfuscateType nameObfuscateType, ObfuscateType codeInjectObfuscateType, ObfuscateNameType obfuscateNameType, int garbageMethodMultiplePerClass, int insertMethodCountPerMethod)
        {
            if (Application.isPlaying || EditorApplication.isCompiling)
            {
                Debug.Log("You need stop play mode or wait compiling finished");
                return;
            }

            string AssemblyPath = assemblyPath[0];


            Debug.Log("Code Obfuscate Start");


            // 按路径读取程序集
            var resolver = new DefaultAssemblyResolver();
            foreach (var item in Const.ResolverSearchDirs)
            {
                resolver.AddSearchDirectory(item);
            }
            var readerParameters = new ReaderParameters { AssemblyResolver = resolver, ReadSymbols = true };

            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(AssemblyPath, readerParameters);

            AssemblyDefinition garbageCodeAssmbly = null;
            if (enableCodeInject)
                garbageCodeAssmbly = AssemblyDefinition.ReadAssembly(uselessCodeLibAssemblyPath, readerParameters);


            if (assembly == null)
            {
                Debug.LogError(string.Format("Code Obfuscate Load assembly failed: {0}", AssemblyPath));
                return;
            }
            try
            {
                ObfuscatorHelper.Init(randomSeed);
                NameObfuscate.Instance.Init(nameObfuscateType);
                CodeInject.Instance.Init(codeInjectObfuscateType, garbageMethodMultiplePerClass, insertMethodCountPerMethod);
                NameFactory.Instance.Load(obfuscateNameType);

                var module = assembly.MainModule;

                //foreach (var item in assembly.MainModule.ModuleReferences)
                //{
                //    Debug.LogError("ModuleReferences:" + item);
                //}
                //foreach (var item in assembly.MainModule.GetTypeReferences())
                //{
                //    Debug.LogError("GetTypeReferences:" + item);
                //}

                //foreach (var item in assembly.MainModule.GetMemberReferences())
                //{
                //    Debug.LogError("GetTypeReferences:" + item);
                //}


                if (enableCodeInject)
                    CodeInject.Instance.DoObfuscate(assembly, garbageCodeAssmbly);
                if (enableNameObfuscate)
                    NameObfuscate.Instance.DoObfuscate(assembly);

                assembly.Write(AssemblyPath, new WriterParameters { WriteSymbols = true });
                Debug.Log("Code Obfuscate Completed!");
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Code Obfuscate failed: {0}", ex));
            }
            finally
            {

                if (assembly != null && assembly.MainModule.SymbolReader != null)
                {
                    assembly.MainModule.SymbolReader.Dispose();
                }


                if (garbageCodeAssmbly != null && garbageCodeAssmbly.MainModule.SymbolReader != null)
                {
                    garbageCodeAssmbly.MainModule.SymbolReader.Dispose();
                }


                NameFactory.Instance.OutputNameMap(Const.NameMapPath);
            }

        }

    }
}


