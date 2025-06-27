using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Patchers.LogicUpdate;

namespace Tangerine.Patchers.UpdateBehavior
{
    /// <summary>
    /// Contains methods that allow using <see cref="MonoBehaviourSingleton&lt;UpdateManager&gt;"/> without inheriting from <see cref="IManagedUpdateBehavior"/>
    /// </summary>
    public static class TangerineUpdateBehaviorManager
    {
        private readonly static Dictionary<IntPtr, IManagedUpdateBehavior> _updateDict = new();
        private readonly static Dictionary<IntPtr, IManagedFixedUpdateBehavior> _fixedUpdateDict = new();
        private readonly static Dictionary<IntPtr, IManagedLateUpdateBehavior> _lateUpdateDict = new();

        #region Update
        private static IManagedUpdateBehavior GetOrAddUpdate(ITangerineUpdateBehavior p_update)
        {
            if (!_updateDict.TryGetValue(p_update.UpdatePointer, out var logic))
            {
                logic = new(p_update.UpdatePointer);
            }
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(logic);


            return logic;
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.AddUpdate&lt;IManagedUpdateBehavior&gt;(IManagedUpdateBehavior)"/>
        /// </summary>
        /// <param name="p_update">Object registered in Il2Cpp that has an implementation of <see cref="ITangerineUpdateBehavior.UpdateFunc"/></param>
        public static void AddUpdate(ITangerineUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(GetOrAddUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="GameLogicUpdateManager.CheckUpdateContain(ILogicUpdate)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineUpdateBehavior)"/>
        /// <returns>The result of the method call</returns>
        public static bool CheckUpdateContain(ITangerineUpdateBehavior p_update)
        {
            return MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(GetOrAddUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="GameLogicUpdateManager.RemoveUpdate(ILogicUpdate)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineUpdateBehavior)"/>
        public static void RemoveUpdate(ITangerineUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(GetOrAddUpdate(p_update));
            _updateDict.Remove(p_update.UpdatePointer);
        }
        #endregion

        #region FixedUpdate
        private static IManagedFixedUpdateBehavior GetOrAddFixedUpdate(ITangerineFixedUpdateBehavior p_update)
        {
            if (!_fixedUpdateDict.TryGetValue(p_update.UpdatePointer, out var logic))
            {
                logic = new(p_update.UpdatePointer);
            }
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(logic);


            return logic;
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.AddUpdate&lt;ITangerineFixedUpdateBehavior&gt;(ITangerineFixedUpdateBehavior)"/>
        /// </summary>
        /// <param name="p_update">Object registered in Il2Cpp that has an implementation of <see cref="ITangerineFixedUpdateBehavior.FixedUpdateFunc"/></param>
        public static void AddUpdate(ITangerineFixedUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(GetOrAddFixedUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.CheckUpdateContain&lt;ITangerineFixedUpdateBehavior&gt;(ITangerineFixedUpdateBehavior)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineFixedUpdateBehavior)"/>
        /// <returns>The result of the method call</returns>
        public static bool CheckUpdateContain(ITangerineFixedUpdateBehavior p_update)
        {
            return MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(GetOrAddFixedUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.RemoveUpdate&lt;ITangerineFixedUpdateBehavior&gt;(ITangerineFixedUpdateBehavior)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineFixedUpdateBehavior)"/>
        public static void RemoveUpdate(ITangerineFixedUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(GetOrAddFixedUpdate(p_update));
            _fixedUpdateDict.Remove(p_update.UpdatePointer);
        }
        #endregion

        #region LateUpdate
        private static IManagedLateUpdateBehavior GetOrAddLateUpdate(ITangerineLateUpdateBehavior p_update)
        {
            if (!_lateUpdateDict.TryGetValue(p_update.UpdatePointer, out var logic))
            {
                logic = new(p_update.UpdatePointer);
            }
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(logic);


            return logic;
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.AddUpdate&lt;ITangerineLateUpdateBehavior&gt;(ITangerineLateUpdateBehavior)"/>
        /// </summary>
        /// <param name="p_update">Object registered in Il2Cpp that has an implementation of <see cref="ITangerineLateUpdateBehavior.LateUpdateFunc"/></param>
        public static void AddUpdate(ITangerineLateUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.AddUpdate(GetOrAddLateUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.CheckUpdateContain&lt;ITangerineLateUpdateBehavior&gt;(ITangerineLateUpdateBehavior)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineLateUpdateBehavior)"/>
        /// <returns>The result of the method call</returns>
        public static bool CheckUpdateContain(ITangerineLateUpdateBehavior p_update)
        {
            return MonoBehaviourSingleton<UpdateManager>.Instance.CheckUpdateContain(GetOrAddLateUpdate(p_update));
        }

        /// <summary>
        /// Calls <see cref="UpdateManager.RemoveUpdate&lt;ITangerineLateUpdateBehavior&gt;(ITangerineLateUpdateBehavior)"/>
        /// </summary>
        /// <inheritdoc cref="AddUpdate(ITangerineLateUpdateBehavior)"/>
        public static void RemoveUpdate(ITangerineLateUpdateBehavior p_update)
        {
            MonoBehaviourSingleton<UpdateManager>.Instance.RemoveUpdate(GetOrAddLateUpdate(p_update));
            _lateUpdateDict.Remove(p_update.UpdatePointer);
        }
        #endregion
    }
}
