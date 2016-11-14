﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

// Multiple Language Handler
public static class MLH {

    #region Vars

    public const string EN_FILE_PATH = "english";

    public static Dictionary<string, string> dict = new Dictionary<string, string>();

    public enum ioStatusCode {
        success,
        notFound,
        unAuth,
        miscFailure
    }

    private static bool _initializedDict = false;

    public static bool initializedDict { 
        get { return _initializedDict; } 
    }

    private static string _language = "none";

    public static string language {
        get { return _language; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Populates MLH.dict with english->otherlanguage pairs,
    /// using two corresponding files
    /// </summary>
    /// <param name="languageFile">Other language file to use</param>
    /// <param name="englishFile">English language file to use, defaults to 'english'</param>
    /// <returns>ioStatusCode representing what happened</returns>
    public static ioStatusCode populateDict(string languageFile, string englishFile = EN_FILE_PATH) {

        // temporary dictionary in case we screw things up
        var tempDict = new Dictionary<string, string>();

        try {

            // open both english and other language file for reading
            using (StreamReader enFile = File.OpenText(englishFile)) {
                using (StreamReader otherFile = File.OpenText(languageFile)) {

                    // notice we're only dealing with enFile,
                    // so we assume the files are the same length
                    while (!enFile.EndOfStream) {
                        string eng = enFile.ReadLine();
                        string fr = otherFile.ReadLine();
                        if (!tempDict.ContainsKey(eng))
                        {
                            tempDict.Add(
                                eng,   // key -> english line
                                fr // val -> other language line
                            );
                        }
                    }
                }
            }

            // we did it!
            if (!initializedDict) {
                _initializedDict = true;
            }

            // set language to the new language filename we're using
            _language = languageFile;
            
            // copy the temp dict to the real dict
            dict = tempDict.ToDictionary(x => x.Key, x => x.Value); // ~funky lambda crap~

            return ioStatusCode.success;

        } catch (FileNotFoundException) {
            return ioStatusCode.notFound; // not found
        } catch (UnauthorizedAccessException) {
            return ioStatusCode.unAuth; // can't access the file
        } catch (Exception e) {
            Debug.Log(e.Message);

            // file corruption would fall under this category
            return ioStatusCode.miscFailure; // misc error
        }
    }

    /// <summary>
    /// Translates the specified string to english or the other language in MLH.dict
    /// </summary>
    /// <param name="textToTr">string to translate</param>
    /// <param name="englishToOther">
    /// true if translating from English,
    /// false if translating to English
    /// </param>
    /// <returns>The translated string</returns>
    public static string tr(string textToTr) {

        if (language == "english") {
            return textToTr;
        }

        if (!initializedDict) {
            if (populateDict(GameMaster.instance.language) != ioStatusCode.success) {
                return textToTr;
            }
        }

        try {  
            return dict[textToTr];
        } catch { 
            // lookup failed
            return textToTr;
        }
    }

    #endregion
}