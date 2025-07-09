using Il2CppInterop.Runtime.InteropTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tangerine.Patchers.UpdateBehavior
{
    /// <summary>
    /// Mirror interface for implementing <see cref="IManagedFixedUpdateBehavior"/>, since Il2Cpp converted it from an interface into a class.
    /// Objects that implement this can be passed to <see cref="TangerineUpdateBehaviorManager"/>.
    /// </summary>
    public interface ITangerineFixedUpdateBehavior
    {
        /// <summary>
        /// Pointer to the <see cref="Il2CppObjectBase"/> that implements <see cref="FixedUpdateFunc"/>
        /// </summary>
        IntPtr LogicPointer { get; }

        /// <summary>
        /// Mirror method for <see cref="IManagedFixedUpdateBehavior.FixedUpdateFunc"/>
        /// </summary>
        void FixedUpdateFunc();
    }
}
