#if REFERENCE_MODEL
using Microsoft.Extensions.Configuration;
using System;

// These pragmas are here just to quiet the compiler.
// This code is just reference code, it's already been published,
// and it's not worth updating to resolve these CA issues.
#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RockLib.Configuration.ObjectFactory.ReferenceModel
{
   public interface IFoo
   {
      int Bar { get; }
      int Baz(int factor);
      string? Qux { get; set; } // Special case - read/write reference-type property
      event EventHandler? Garply;
   }

   public class ProxyFoo : ConfigReloadingProxy<IFoo>, IFoo
   {
      public ProxyFoo(IConfiguration section, DefaultTypes defaultTypes, ValueConverters valueConverters, Type declaringType, string memberName)
          : base(section, defaultTypes, valueConverters, declaringType, memberName, null)
      {
      }

      // Properties are strictly pass-through.
      int IFoo.Bar => Object.Bar;

      // Methods are strictly pass-through.
      int IFoo.Baz(int qux) => Object.Baz(qux);

      // Properties are strictly pass-through.
      string? IFoo.Qux { get => Object.Qux; set => Object.Qux = value; }

      // Events are pass-through, but also need to capture handlers in a private field.
      private EventHandler? _garply;
      event EventHandler? IFoo.Garply
      {
         add
         {
            Object.Garply += value;
            _garply += value;
         }
         remove
         {
            Object.Garply -= value;
            _garply -= value;
         }
      }

      protected override void TransferState(IFoo oldObject, IFoo newObject)
      {
         if(oldObject is not null && newObject is not null)
         {
            // Event handlers from the old object need to be copied to the new one.
            newObject.Garply += _garply;

            // Special case for when the interface has a read/write property: if the new
            // property value is null and the old property value is not null, then copy
            // the value from old to new.
            if (oldObject.Qux is not null && newObject.Qux is null)
            {
               newObject.Qux = oldObject.Qux;
            }
         }
      }
   }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1033 // Interface methods should be callable by child types
#endif
