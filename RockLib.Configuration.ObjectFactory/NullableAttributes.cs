// The point of the types in this file are to "patch" in the nullable attributes
// (https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis)
// for .NET 4.8. This code came from https://source.dot.net/#System.Private.CoreLib/NullableAttributes.cs

#if NET48
namespace System.Diagnostics.CodeAnalysis
{
   [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
   internal sealed class MaybeNullWhenAttribute
      : Attribute
   {
      /// <summary>Initializes the attribute with the specified return value condition.</summary>
      /// <param name="returnValue">
      /// The return value condition. If the method returns this value, the associated parameter may be null.
      /// </param>
      public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

      /// <summary>Gets the return value condition.</summary>
      public bool ReturnValue { get; }
   }
}
#endif