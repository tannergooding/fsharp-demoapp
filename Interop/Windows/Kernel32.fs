namespace DemoApplication.Interop

open System.Runtime.InteropServices

module Kernel32 =
    // External Methods
    [<DllImport("Kernel32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern HMODULE GetModuleHandle(
        [<In; Optional>] LPCWSTR lpModuleName
    )

    [<DllImport("Kernel32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "QueryPerformanceCounter", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL QueryPerformanceCounter(
        [<Out>] LARGE_INTEGER& lpPerformanceCount
    )

    [<DllImport("Kernel32", BestFitMapping = false, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, EntryPoint = "QueryPerformanceFrequency", ExactSpelling = true, PreserveSig = true, SetLastError = true, ThrowOnUnmappableChar = false)>]
    extern BOOL QueryPerformanceFrequency(
        [<Out>] LARGE_INTEGER& lpFrequency
    )
