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
using System.Globalization;
using System.Linq;
using SweetSugar.Scripts.Level;
using SweetSugar.Scripts.Localization;
using SweetSugar.Scripts.System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class LanguageSelectionGame : UnityEngine.MonoBehaviour
    {
        private List<CultureInfo> cultures;
        private TMP_Dropdown Dropdown;
        public CurtureTuple[] extraLanguages;
        private void Start()
        {
            Dropdown = GetComponent<TMP_Dropdown>();
            var txt = Resources.LoadAll<TextAsset>("Localization/");
            cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(i => txt.Any(x => x.name == i.DisplayName)).ToList();
            cultures.AddRange(extraLanguages.Select(i=>new CultureInfo(i.culture)));
            Dropdown.options = cultures.Select(i => new TMP_Dropdown.OptionData(i.Name.ToUpper())).ToList();
            var _debugSettings = Resources.Load("Scriptable/DebugSettings") as DebugSettings;
            Dropdown.captionText.text = cultures.First(i => i.EnglishName == LocalizationManager.GetSystemLanguage(_debugSettings).ToString()).Name.ToUpper();
            Dropdown.value = cultures.ToList().FindIndex(i => i.EnglishName == LocalizationManager.GetSystemLanguage(_debugSettings).ToString());
        }

        public void OnChangeLanguage()
        {
            LocalizationManager.LoadLanguage(GetSelectedLanguage().EnglishName);
            CrosssceneData.selectedLanguage = GetSelectedLanguage().EnglishName;
        }

        private CultureInfo GetSelectedLanguage()
        {
            return cultures.ToArray()[Dropdown.value];
        }
    }

    [Serializable]
    public struct CurtureTuple
    {
        public string culture;
        public string name;
    }
}