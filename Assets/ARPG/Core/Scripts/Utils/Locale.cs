using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARCeye
{
    [Serializable]
    public enum Locale
    {
        [InspectorName("en_US")]
        en_US,
        [InspectorName("ko_KR")]
        ko_KR,
        [InspectorName("zh_CN")]
        zh_CN,
        [InspectorName("zh_TW")]
        zh_TW,
        [InspectorName("ja_JP")]
        ja_JP,
        [InspectorName("fr_FR")]
        fr_FR,
        [InspectorName("de_DE")]
        de_DE,
        [InspectorName("it_IT")]
        it_IT,
        [InspectorName("pt_BR")]
        pt_BR,
        [InspectorName("es_ES")]
        es_ES
    }

    public static class LocaleConverter
    {
        public static string GetLanguageCode(Locale locale)
        {
            string localeString = locale.ToString();
            var localeCode = localeString.Split('_');
            return localeCode[0];
        }

        public static string GetCountryCode(Locale locale)
        {
            string localeString = locale.ToString();
            var localeCode = localeString.Split('_');
            return localeCode[1];
        }
    }
}