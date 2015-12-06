using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TouchDashPC
{
    public class IniFile
    {
        #region Constructors
        /// <summary>
        /// Create a new IniHelper instance.
        /// </summary>
        public IniFile(string filePath)
            : this(filePath, false)
        { }

        /// <summary>
        /// Create a new IniHelper instance.
        /// </summary>
        public IniFile(string filePath, bool createFile)
        {
            if (!createFile && !File.Exists(filePath))
            {
                throw new FileNotFoundException();
            }

            if (createFile)
            {
                File.Create(filePath);
            }

            this.FilePath = filePath;
            this.status = IniStatus.Unchanged;

            this.GetProfileString();
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the entire section through a given index.
        /// </summary>
        public virtual IniSection this[string section]
        {
            get
            {
                if (!this.sections.ContainsKey(section)
                    || this.sections[section].Status == IniStatus.Removed)
                {
                    throw new IndexOutOfRangeException();
                }

                return this.sections[section];
            }
        }

        public virtual string this[string section, string key]
        {
            get
            {
                if (!this.sections.ContainsKey(section)
                    || this.sections[section].Status == IniStatus.Removed)
                {
                    throw new IndexOutOfRangeException();
                }

                return this.sections[section][key];
            }
        }
        #endregion

        #region Properties
        private string filePath;
        /// <summary>
        /// Get initialization file path.
        /// </summary>
        public virtual string FilePath
        {
            get { return this.filePath; }
            protected set { this.filePath = value; }
        }

        private IniStatus status;
        /// <summary>
        /// Gets a value indicating whether there are unsaved changes.
        /// </summary>
        public virtual IniStatus Status
        {
            get { return this.status; }
            protected set { this.status = value; }
        }

        private Dictionary<string, IniSection> sections;
        /// <summary>
        /// Gets all ini sections
        /// </summary>
        public virtual List<IniSection> Sections
        {
            get { return this.sections.Values.ToList(); }
        }
        #endregion

        #region External DLL's
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
        #endregion

        #region Public Methods
        /// <summary>
        /// Remove the entire section, according to the given argument
        /// </summary>
        public virtual void Remove(string section)
        {
            if (this.sections[section] == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.sections[section].Remove();
            this.status = IniStatus.Modified;
        }

        /// <summary>
        /// Remove the section key, according to the given arguments
        /// </summary>
        public virtual void Remove(string section, string key)
        {
            if (this.sections[section] == null || this.sections[section][key] == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.sections[section].Remove(key);
            this.status = IniStatus.Modified;
        }

        /// <summary>
        /// Insert a new section, without keys
        /// </summary>
        public virtual void Insert(string section)
        {
            if (this.sections[section] != null)
            {
                throw new InvalidOperationException();
            }

            this.sections.Add(section, new IniSection(section));
            this.status = IniStatus.Modified;
        }

        public virtual String Get(string section, string key)
        {
            if (this.sections[section] == null)
            {
                throw new InvalidOperationException();
            }
            return this.sections[section][key];
        }

        public virtual bool GetBool(string section, string key)
        {
            if (this.sections[section] == null)
            {
                throw new InvalidOperationException();
            }
            string value = this.sections[section][key];
            if (value.Equals("true"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual int GetInt(string section, string key)
        {
            if (this.sections[section] == null)
            {
                throw new InvalidOperationException();
            }
            string value = this.sections[section][key];
            return int.Parse(value);
        }


        public virtual void Set(string section, string key, object value)
        {
            if (this.sections[section] == null)
            {
                this.sections.Add(section, new IniSection(section));
            }
            if (value.ToString().ToLower().Equals("true") || value.ToString().ToLower().Equals("false"))
            {
                this.sections[section][key] = value.ToString().ToLower();
            }
            else
            {
                this.sections[section][key] = value.ToString();
            }
            this.status = IniStatus.Modified;
            WriteToFile();
        }



        /// <summary>
        /// Insert a new key value pair to a given section. If the section doesn't exist will be created.
        /// </summary>
        public virtual void Insert(string section, string key, string value)
        {
            if (this.sections[section] == null)
            {
                this.sections.Add(section, new IniSection(section));
            }

            if (this.sections[section][key] != null)
            {
                throw new InvalidOperationException();
            }

            this.sections[section][key] = value;
            this.status = IniStatus.Modified;
        }

        /// <summary>
        /// Write/rewrite the instace file
        /// </summary>
        public virtual void WriteToFile()
        {
            if (this.status == IniStatus.Unchanged)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, IniSection> section in this.sections)
            {
                if (section.Value.Status != IniStatus.Removed)
                {
                    sb.Append("[" + section.Key + "]");
                    sb.Append(Environment.NewLine);
                    foreach (IniSection.IniKeyValuePair keyValue in section.Value.KeyValuePairs)
                    {
                        if (keyValue.Status != IniStatus.Removed)
                        {
                            sb.Append(keyValue.Key + "=" + keyValue.Value);
                            sb.Append(Environment.NewLine);
                        }
                    }
                }
            }
            System.IO.File.WriteAllText(filePath, sb.ToString());
            this.status = IniStatus.Unchanged;
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Gets the initilization file content.
        /// </summary>
        protected void GetProfileString()
        {
            this.sections = new Dictionary<string, IniSection>(StringComparer.CurrentCultureIgnoreCase);

            string currentSection = String.Empty;
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (sr.Peek() >= 0)
                {
                    string fileLine = sr.ReadLine();

                    if (!String.IsNullOrEmpty(fileLine) && !String.IsNullOrWhiteSpace(fileLine))
                    {
                        if (fileLine.Trim().StartsWith("["))
                        {
                            currentSection = fileLine.Substring(1, fileLine.Length - 2);
                            this.sections.Add(currentSection, new IniSection(currentSection));
                        }
                        else
                        {
                            string[] keyValue = fileLine.Split('=');
                            this.sections[currentSection].Add(keyValue[0], keyValue[1]);
                        }
                    }
                }
            }
        }
        #endregion

        public class IniSection
        {
            #region Constructors
            public IniSection(string name)
                : this(name, null)
            { }

            public IniSection(string name, IDictionary<string, IniKeyValuePair> keyValuePairs)
            {
                this.name = name;

                if (keyValuePairs == null)
                {
                    this.keyValuePairs = new Dictionary<string, IniKeyValuePair>();
                }
                else
                {
                    this.keyValuePairs = keyValuePairs;
                }
            }
            #endregion

            #region Properties
            private string name;
            public virtual string Name
            {
                get { return this.name; }
                protected set
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException();
                    }

                    this.name = value;
                }
            }

            private IDictionary<string, IniKeyValuePair> keyValuePairs;
            public virtual ICollection<IniKeyValuePair> KeyValuePairs
            {
                get { return this.keyValuePairs.Values; }
            }

            private IniStatus status;
            public virtual IniStatus Status
            {
                get { return this.status; }
                protected set { this.status = value; }
            }
            #endregion

            #region Indexer
            public virtual string this[string key]
            {
                get
                {
                    if (!this.keyValuePairs.ContainsKey(key))
                    {
                        throw new ArgumentNullException();
                    }

                    return this.keyValuePairs[key].Value;
                }
                set
                {
                    if (!this.keyValuePairs.ContainsKey(key))
                    {
                        throw new ArgumentNullException();
                    }

                    this.status = IniStatus.Modified;
                    this.keyValuePairs[key].Value = value;
                }
            }
            #endregion

            #region Public Methods
            public virtual void Add(string key)
            {
                this.Add(key, "");
            }

            public virtual void Add(string key, string value)
            {
                this.status = IniStatus.Modified;
                this.keyValuePairs.Add(key, new IniKeyValuePair(this, key, value, IniStatus.Inserted));
            }

            public virtual void Remove()
            {
                this.status = IniStatus.Removed;
            }

            public virtual void Remove(string key)
            {
                if (String.IsNullOrEmpty(key) || !this.keyValuePairs.ContainsKey(key))
                {
                    throw new ArgumentException();
                }

                this.status = IniStatus.Modified;
                this.keyValuePairs[key].Remove();
            }
            #endregion

            public class IniKeyValuePair
            {
                #region Constructors
                public IniKeyValuePair(IniSection parent, string key, string value)
                    : this(parent, key, value, IniStatus.Unchanged)
                { }

                public IniKeyValuePair(IniSection parent, string key, string value, IniStatus status)
                {
                    this.parent = parent;
                    this.key = key;
                    this.keyValue = value;
                    this.status = status;
                }
                #endregion

                #region Properties
                private IniSection parent;
                public virtual IniSection Parent
                {
                    get { return this.parent; }
                    protected set { this.parent = value; }
                }

                private string key;
                public virtual string Key
                {
                    get { return this.key; }
                    protected set { this.key = value; }
                }

                private string keyValue;
                public virtual string Value
                {
                    get { return this.keyValue; }
                    set
                    {
                        this.keyValue = value;
                        this.status = IniStatus.Modified;
                    }
                }

                private IniStatus status;
                public virtual IniStatus Status
                {
                    get { return this.status; }
                }
                #endregion

                #region Public Methods
                public virtual void Remove()
                {
                    this.status = IniStatus.Removed;
                    this.parent.Status = IniStatus.Modified;
                }
                #endregion
            }
        }

        /// <summary>
        /// IniFile section status.
        /// </summary>
        public enum IniStatus
        {
            Unchanged,
            Inserted,
            Modified,
            Removed
        }
    }
}