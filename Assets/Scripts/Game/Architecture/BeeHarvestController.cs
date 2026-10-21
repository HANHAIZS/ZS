using QFramework;
using UnityEngine;

namespace BeeHarvest
{
    public abstract class BeeHarvestController : MonoBehaviour, IController
    {
        /// <summary>
        /// Returns the single project-owned QFramework architecture.
        /// </summary>
        public IArchitecture GetArchitecture()
        {
            return BeeHarvestApp.Interface;
        }
    }
}
