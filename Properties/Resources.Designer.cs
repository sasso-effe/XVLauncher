﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XVLauncher.Properties {
    using System;
    
    
    /// <summary>
    ///   Classe di risorse fortemente tipizzata per la ricerca di stringhe localizzate e così via.
    /// </summary>
    // Questa classe è stata generata automaticamente dalla classe StronglyTypedResourceBuilder.
    // tramite uno strumento quale ResGen o Visual Studio.
    // Per aggiungere o rimuovere un membro, modificare il file con estensione ResX ed eseguire nuovamente ResGen
    // con l'opzione /str oppure ricompilare il progetto VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Restituisce l'istanza di ResourceManager nella cache utilizzata da questa classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("XVLauncher.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Esegue l'override della proprietà CurrentUICulture del thread corrente per tutte le
        ///   ricerche di risorse eseguite utilizzando questa classe di risorse fortemente tipizzata.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a #00DDDDDD.
        /// </summary>
        internal static string ButtonBG {
            get {
                return ResourceManager.GetString("ButtonBG", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a #FF406380.
        /// </summary>
        internal static string ButtonBorder {
            get {
                return ResourceManager.GetString("ButtonBorder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a #FF062640.
        /// </summary>
        internal static string ButtonSelectedBG {
            get {
                return ResourceManager.GetString("ButtonSelectedBG", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a #FF1B86FF.
        /// </summary>
        internal static string ButtonSelectedBorder {
            get {
                return ResourceManager.GetString("ButtonSelectedBorder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a Test.exe.
        /// </summary>
        internal static string Executable {
            get {
                return ResourceManager.GetString("Executable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a .
        /// </summary>
        internal static string InstallDirectory {
            get {
                return ResourceManager.GetString("InstallDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a 0.5.0.
        /// </summary>
        internal static string LauncherVersion {
            get {
                return ResourceManager.GetString("LauncherVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a TestApp.
        /// </summary>
        internal static string SaveDirectory {
            get {
                return ResourceManager.GetString("SaveDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Cerca una stringa localizzata simile a https://gitlab.com/sasso-effe/xvlauncher-test/-/raw/{0}/.
        /// </summary>
        internal static string UpdateUrl {
            get {
                return ResourceManager.GetString("UpdateUrl", resourceCulture);
            }
        }
    }
}