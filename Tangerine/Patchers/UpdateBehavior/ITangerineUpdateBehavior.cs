using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Patchers.UpdateBehavior
{
    /// <summary>
    /// Mirror interface for implementing <see cref="IManagedUpdateBehavior"/>, since Il2Cpp converted it from an interface into a class.
    /// Objects that implement this can be passed to <see cref="TangerineUpdateBehaviorManager"/>.
    /// </summary>
    public interface ITangerineUpdateBehavior
    {
        /// <summary>
        /// Pointer to the <see cref="Il2CppObjectBase"/> that implements <see cref="IManagedUpdateBehavior"/>
        /// </summary>
        IntPtr UpdatePointer { get; }

        /// <summary>
        /// Mirror method for <see cref="IManagedUpdateBehavior.UpdateFunc"/>
        /// </summary>
        void UpdateFunc();
    }
}
