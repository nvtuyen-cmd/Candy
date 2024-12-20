// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SweetSugar.Scripts.MapScripts
{
    public class LevelsMap : MonoBehaviour {
        public static LevelsMap _instance;
        public static IMapProgressManager _mapProgressManager = new PlayerPrefsMapProgressManager ();

        public bool IsGenerated;

        public Transform CharacterPrefab;
        public int Count = 10;

        public TranslationType TranslationType;

        public bool StarsEnabled;
        public StarsType StarsType;

        public bool ScrollingEnabled;
        public MapCamera MapCamera;
        public bool IsClickEnabled;
        public bool IsConfirmationEnabled;

        public void Awake () {
            _instance = this;
        }

        public void OnDestroy () {
            _instance = null;
        }

        public void OnEnable () {
            if (IsGenerated) {
                Reset ();
            }
        }

        public void Reset()
        {
            UpdateMapLevels();
            PlaceCharacterToLastUnlockedLevel();
            SetCameraToCharacter();
        }

        private void UpdateMapLevels()
        {
        }

        private void PlaceCharacterToLastUnlockedLevel()
        {
          
        }
        private void SetCameraToCharacter()
        {
        }

        #region Events

        public static event EventHandler<LevelReachedEventArgs> LevelSelected;

        #endregion

        #region Static API

        public static void CompleteLevel(int number)
        {
            CompleteLevelInternal(number, 1);
        }

        public static void CompleteLevel(int number, int starsCount)
        {
            CompleteLevelInternal(number, starsCount);
        }

        internal static void OnLevelSelected(int number)
        {
            if (LevelSelected != null && !IsLevelLocked(number))  //need to fix in the map plugin
                LevelSelected(_instance, new LevelReachedEventArgs(number));

            if (!_instance.IsConfirmationEnabled)
                GoToLevel(number);
        }

        public static void GoToLevel(int number)
        {
            switch (_instance.TranslationType)
            {
                case TranslationType.Teleportation:
                    _instance.TeleportToLevelInternal(number, false);
                    break;
                case TranslationType.Walk:
                    _instance.WalkToLevelInternal(number);
                    break;
            }
        }

        public static bool IsLevelLocked(int number)
        {
            return number > 1 && _mapProgressManager.LoadLevelStarsCount(number - 1) == 0;
        }

        public static void OverrideMapProgressManager(IMapProgressManager mapProgressManager)
        {
            _mapProgressManager = mapProgressManager;
        }

        public static void ClearAllProgress()
        {
            _instance.ClearAllProgressInternal();
        }

        public static bool IsStarsEnabled()
        {
            return _instance.StarsEnabled;
        }

        public static bool GetIsClickEnabled()
        {
            return _instance.IsClickEnabled;
        }

        public static bool GetIsConfirmationEnabled()
        {
            return _instance.IsConfirmationEnabled;
        }

        #endregion

        private static void CompleteLevelInternal(int number, int starsCount)
        {
            if (IsLevelLocked(number))
            {
                Debug.Log(string.Format("Can't complete locked level {0}.", number));
            }
            else if (starsCount < 1 || starsCount > 3)
            {
                Debug.Log(string.Format("Can't complete level {0}. Invalid stars count {1}.", number, starsCount));
            }
            else
            {
                int curStarsCount = _mapProgressManager.LoadLevelStarsCount(number);
                int maxStarsCount = Mathf.Max(curStarsCount, starsCount);
                _mapProgressManager.SaveLevelStarsCount(number, maxStarsCount);

                if (_instance != null)
                    _instance.UpdateMapLevels();
            }
        }

        private void TeleportToLevelInternal(int number, bool isQuietly)
        {
            
        }
    
        public delegate void ReachedLevelEvent();
        public static ReachedLevelEvent OnLevelReached;

        private void WalkToLevelInternal(int number)
        {
            
        }
        private void ClearAllProgressInternal()
        {
            Reset();
        }

        public void SetStarsEnabled(bool bEnabled)
        {
            StarsEnabled = bEnabled;
        }

        public void SetStarsType(StarsType starsType)
        {
            StarsType = starsType;
        }

    }
}