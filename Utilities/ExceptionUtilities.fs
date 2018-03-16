namespace DemoApplication.Utilities

open DemoApplication
open System
open System.Runtime.InteropServices
open System.Threading

module ExceptionUtilities =
    // Static Methods
    let public NewArgumentNullException(argName:string) : ArgumentNullException =
        let message = String.Format(Resources.ArgumentNullExceptionMessage, argName)
        new ArgumentNullException(argName, message)

    let public NewArgumentOutOfRangeException(argName:string, value:obj) : ArgumentOutOfRangeException =
        let message = String.Format(Resources.ArgumentOutOfRangeExceptionMessage, argName, value)
        new ArgumentOutOfRangeException(argName, value, message)

    let public NewExternalException(methodName:string, errorCode:int32) : ExternalException =
        let message = String.Format(Resources.ExternalExceptionMessage, methodName, errorCode)
        new ExternalException(message, errorCode)

    let public NewExternalExceptionForLastError(methodName:string) : ExternalException =
        let errorCode = Marshal.GetLastWin32Error()
        NewExternalException(methodName, errorCode)

    let public NewInvalidOperationException(name:string, value:obj) : InvalidOperationException =
        let message = String.Format(Resources.InvalidOperationExceptionMessage, name, value)
        new InvalidOperationException(message)

    let public ThrowArgumentNullException(argName:string) : unit =
        raise (NewArgumentNullException(argName))

    let public ThrowArgumentOutOfRangeException(argName:string, value:obj) : unit =
        raise (NewArgumentOutOfRangeException(argName, value))

    let public ThrowExternalException(methodName:string, errorCode:int32) : unit =
        raise (NewExternalException(methodName, errorCode))

    let public ThrowExternalExceptionForLastError(methodName:string) : unit =
        raise (NewExternalExceptionForLastError(methodName))

    let public ThrowIfNull(paramName:string, arg:'T when 'T : not struct) : unit =
        match arg with
        | null -> ThrowArgumentNullException paramName
        | _ -> ()

    let public ThrowInvalidOperationException(name:string, value:obj) : unit =
        raise (NewInvalidOperationException(name, value))

    let public ThrowIfNotThread(thread:Thread) : unit =
        if Thread.CurrentThread <> thread then
            ThrowInvalidOperationException("thread", thread)
        ()
