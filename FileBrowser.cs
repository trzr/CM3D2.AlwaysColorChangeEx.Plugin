using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

/*
    File browser for selecting files or folders at runtime.
 */

public enum FileBrowserType
{
    File,
    Directory
}

public class FileBrowser
{

    // Called when the user clicks cancel or select
    public delegate void FinishedCallback(string path);
    // Defaults to working directory
    public string CurrentDirectory
    {
        get
        {
            return m_currentDirectory;
        }
        set
        {
            SetNewDirectory(value);
            SwitchDirectoryNow();
        }
    }
    protected string m_currentDirectory;
    // Optional pattern for filtering selectable files/folders. See:
    // http://msdn.microsoft.com/en-us/library/wz42302f(v=VS.90).aspx
    // and
    // http://msdn.microsoft.com/en-us/library/6ff71z1w(v=VS.90).aspx
    public string[] SelectionPatterns
    {
        get
        {
            return m_filePatterns;
        }
        set
        {
            m_filePatterns = value;
            ReadDirectoryContents();
        }
    }
    protected string[] m_filePatterns;

    // Optional image for directories
    public Texture2D DirectoryImage
    {
        get
        {
            return m_directoryImage;
        }
        set
        {
            m_directoryImage = value;
            BuildContent();
        }
    }
    protected Texture2D m_directoryImage;

    // Optional image for files
    public Texture2D FileImage
    {
        get
        {
            return m_fileImage;
        }
        set
        {
            m_fileImage = value;
            BuildContent();
        }
    }
    protected Texture2D m_fileImage;

    // Browser type. Defaults to File, but can be set to Folder
    public FileBrowserType BrowserType
    {
        get
        {
            return m_browserType;
        }
        set
        {
            m_browserType = value;
            ReadDirectoryContents();
        }
    }
    protected FileBrowserType m_browserType;
    protected string m_newDirectory;
    protected string[] m_currentDirectoryParts;

    protected string[] m_files;
    protected GUIContent[] m_filesWithImages;
    protected int m_selectedFile;

    protected string[] m_nonMatchingFiles;
    protected GUIContent[] m_nonMatchingFilesWithImages;
    protected int m_selectedNonMatchingDirectory;

    protected string[] m_directories;
    protected GUIContent[] m_directoriesWithImages;
    protected int m_selectedDirectory;

    protected string[] m_nonMatchingDirectories;
    protected GUIContent[] m_nonMatchingDirectoriesWithImages;

    protected bool m_currentDirectoryMatches;

    protected GUIStyle CentredText
    {
        get
        {
            if (m_centredText == null)
            {
                m_centredText = new GUIStyle(GUI.skin.label);
                m_centredText.alignment = TextAnchor.MiddleLeft;
                m_centredText.fixedHeight = GUI.skin.button.fixedHeight;
            }
            return m_centredText;
        }
    }
    protected GUIStyle m_centredText;

    protected string m_name;
    protected Rect m_screenRect;

    protected Vector2 m_scrollPosition;

    protected FinishedCallback m_callback;

    // Browsers need at least a rect, name and callback
    public FileBrowser(Rect screenRect, string name, FinishedCallback callback)
    {
        m_name = name;
        m_screenRect = screenRect;
        m_browserType = FileBrowserType.File;
        m_callback = callback;
        SetNewDirectory(Directory.GetCurrentDirectory());
        SwitchDirectoryNow();
    }

    protected void SetNewDirectory(string directory)
    {
        m_newDirectory = directory;
    }

    protected void SwitchDirectoryNow()
    {
        if (m_newDirectory == null || m_currentDirectory == m_newDirectory)
        {
            return;
        }
        m_currentDirectory = m_newDirectory;
        m_scrollPosition = Vector2.zero;
        m_selectedDirectory = m_selectedNonMatchingDirectory = m_selectedFile = -1;
        ReadDirectoryContents();
    }

    protected void ReadDirectoryContents()
    {
        if (m_currentDirectory == "/")
        {
            m_currentDirectoryParts = new string[] { "" };
            m_currentDirectoryMatches = false;
        }
        else
        {
            m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
            if (SelectionPatterns != null)
            {
                m_currentDirectoryMatches = false;
                foreach (string pattern in SelectionPatterns)
                {
                    string directoryName = Path.GetDirectoryName(m_currentDirectory);
                    if (directoryName != null)
                    {
                        string[] generation = Directory.GetDirectories(
                            directoryName,
                            pattern
                        );
                        m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
                        if (m_currentDirectoryMatches)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                m_currentDirectoryMatches = false;
            }
        }

        if (BrowserType == FileBrowserType.File || SelectionPatterns == null)
        {
            m_directories = Directory.GetDirectories(m_currentDirectory);
            m_nonMatchingDirectories = new string[0];
        }
        else
        {
            List<string> list = new List<string>();
            foreach (var pattern in SelectionPatterns)
            {
                var arr = Directory.GetDirectories(m_currentDirectory, pattern);
                list.AddRange(arr);
            }
            m_directories = list.ToArray();
            var nonMatchingDirectories = new List<string>();
            foreach (string directoryPath in Directory.GetDirectories(m_currentDirectory))
            {
                if (Array.IndexOf(m_directories, directoryPath) < 0)
                {
                    nonMatchingDirectories.Add(directoryPath);
                }
            }
            m_nonMatchingDirectories = nonMatchingDirectories.ToArray();
            for (int i = 0; i < m_nonMatchingDirectories.Length; ++i)
            {
                int lastSeparator = m_nonMatchingDirectories[i].LastIndexOf(Path.DirectorySeparatorChar);
                m_nonMatchingDirectories[i] = m_nonMatchingDirectories[i].Substring(lastSeparator + 1);
            }
            Array.Sort(m_nonMatchingDirectories);
        }

        for (int i = 0; i < m_directories.Length; ++i)
        {
            m_directories[i] = m_directories[i].Substring(m_directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        if (BrowserType == FileBrowserType.Directory || SelectionPatterns == null)
        {
            m_files = Directory.GetFiles(m_currentDirectory);
            m_nonMatchingFiles = new string[0];
        }
        else
        {
            m_files = new string[0];
            if (SelectionPatterns != null)
            {
                List<string> list = new List<string>();
                if (m_files != null)
                {
                    list.AddRange(m_files);
                }
                foreach (string pattern in SelectionPatterns)
                {
                    var files = Directory.GetFiles(m_currentDirectory, pattern);
                    if (files.Length > 0)
                    {
                        list.AddRange(files);
                    }
                }
                m_files = list.ToArray();
            }
            var nonMatchingFiles = new List<string>();
            foreach (string filePath in Directory.GetFiles(m_currentDirectory))
            {
                if (Array.IndexOf(m_files, filePath) < 0)
                {
                    nonMatchingFiles.Add(filePath);
                }
            }
            m_nonMatchingFiles = nonMatchingFiles.ToArray();
            for (int i = 0; i < m_nonMatchingFiles.Length; ++i)
            {
                m_nonMatchingFiles[i] = Path.GetFileName(m_nonMatchingFiles[i]);
            }
            Array.Sort(m_nonMatchingFiles);
        }
        for (int i = 0; i < m_files.Length; ++i)
        {
            m_files[i] = Path.GetFileName(m_files[i]);
        }
        Array.Sort(m_files);
        BuildContent();
        m_newDirectory = null;
    }

    protected void BuildContent()
    {
        m_directoriesWithImages = new GUIContent[m_directories.Length];
        for (int i = 0; i < m_directoriesWithImages.Length; ++i)
        {
            m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
        }
        m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
        for (int i = 0; i < m_nonMatchingDirectoriesWithImages.Length; ++i)
        {
            m_nonMatchingDirectoriesWithImages[i] = new GUIContent(m_nonMatchingDirectories[i], DirectoryImage);
        }
        m_filesWithImages = new GUIContent[m_files.Length];
        for (int i = 0; i < m_filesWithImages.Length; ++i)
        {
            m_filesWithImages[i] = new GUIContent(m_files[i], FileImage);
        }
        m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
        for (int i = 0; i < m_nonMatchingFilesWithImages.Length; ++i)
        {
            m_nonMatchingFilesWithImages[i] = new GUIContent(m_nonMatchingFiles[i], FileImage);
        }
    }

    public void OnGUI()
    {
        GUILayout.BeginArea(m_screenRect, m_name, GUI.skin.window);
        GUILayout.BeginHorizontal();
        try {
            for (int parentIndex = 0; parentIndex < m_currentDirectoryParts.Length; ++parentIndex) {
                if (parentIndex == m_currentDirectoryParts.Length - 1) {
                    GUILayout.Label(m_currentDirectoryParts[parentIndex], CentredText);
    
                } else if (GUILayout.Button(m_currentDirectoryParts[parentIndex])) {
                    string parentDirectoryName = m_currentDirectory;
                    for (int i = m_currentDirectoryParts.Length - 1; i > parentIndex; --i) {
                        parentDirectoryName = Path.GetDirectoryName(parentDirectoryName);
                    }
                    SetNewDirectory(parentDirectoryName);
                }
            }
    
            GUILayout.FlexibleSpace();
        } finally {
            GUILayout.EndHorizontal();
        }
        m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, true, 
                                                     GUI.skin.horizontalScrollbar, 
                                                     GUI.skin.verticalScrollbar, GUI.skin.box );
        m_selectedDirectory = GUILayoutx.SelectionList(m_selectedDirectory, m_directoriesWithImages, 
                                                       DirectoryClickCallback);
        if (m_selectedDirectory > -1) {
            m_selectedFile = m_selectedNonMatchingDirectory = -1;
        }

        m_selectedNonMatchingDirectory = GUILayoutx.SelectionList(
            m_selectedNonMatchingDirectory,
            m_nonMatchingDirectoriesWithImages,
            NonMatchingDirectoryClickCallback
        );
        if (m_selectedNonMatchingDirectory > -1) {
            m_selectedDirectory = m_selectedFile = -1;
        }
        GUI.enabled = BrowserType == FileBrowserType.File;
        m_selectedFile = GUILayoutx.SelectionList(
            m_selectedFile,
            m_filesWithImages,
            FileClickCallback
        );
        GUI.enabled = true;
        if (m_selectedFile > -1) {
            m_selectedDirectory = m_selectedNonMatchingDirectory = -1;
        }
        GUI.enabled = false;
        GUILayoutx.SelectionList( -1, m_nonMatchingFilesWithImages );
        GUI.enabled = true;

        GUILayout.EndScrollView();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("キャンセル", GUILayout.Width(120))) {
            m_callback(null);
        }
        if (BrowserType == FileBrowserType.File) {
            GUI.enabled = m_selectedFile > -1;
        } else {
            if (SelectionPatterns == null) {
                GUI.enabled = m_selectedDirectory > -1;
            } else {
                GUI.enabled = m_selectedDirectory > -1 ||
                    (
                        m_currentDirectoryMatches &&
                        m_selectedNonMatchingDirectory == -1 &&
                        m_selectedFile == -1
                    );
            }
        }

        if (GUILayout.Button("選択", GUILayout.Width(120))) {
            if (BrowserType == FileBrowserType.File) {
                m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
            } else {
                if (m_selectedDirectory > -1) {
                    m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
                } else {
                    m_callback(m_currentDirectory);
                }
            }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        if (Event.current.type == EventType.Repaint) {
            SwitchDirectoryNow();
        }
    }

    protected void FileClickCallback(int i)
    {
        if (BrowserType == FileBrowserType.File)
        {
            //            m_callback(Path.Combine(m_currentDirectory, m_files[i]));
        }
    }

    protected void DirectoryClickCallback(int i)
    {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
    }

    protected void NonMatchingDirectoryClickCallback(int i)
    {
        SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
    }

}


public class GUILayoutx
{

    public delegate void ClickCallback(int index);

    public static int SelectionList(int selected, GUIContent[] list)
    {
        return SelectionList(selected, list, "Label", null);
    }

    public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle)
    {
        return SelectionList(selected, list, elementStyle, null);
    }

    public static int SelectionList(int selected, GUIContent[] list, ClickCallback callback)
    {
        return SelectionList(selected, list, "Label", callback);
    }

    public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle, ClickCallback callback)
    {
        for (int i = 0; i < list.Length; ++i)
        {
            Rect elementRect = GUILayoutUtility.GetRect(list[i], elementStyle);
            bool hover = elementRect.Contains(Event.current.mousePosition);
            if (hover && Event.current.type == EventType.MouseDown)
            {
                selected = i;
                callback(i);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.repaint)
            {
                elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
            }
        }
        return selected;
    }

    public static int SelectionList(int selected, string[] list)
    {
        return SelectionList(selected, list, "Label", null);
    }

    public static int SelectionList(int selected, string[] list, GUIStyle elementStyle)
    {
        return SelectionList(selected, list, elementStyle, null);
    }

    public static int SelectionList(int selected, string[] list, ClickCallback callback)
    {
        return SelectionList(selected, list, "Label", callback);
    }

    public static int SelectionList(int selected, string[] list, GUIStyle elementStyle, ClickCallback callback)
    {
        elementStyle.active.textColor = new Color(0.8f, 1f, 1f);
        for (int i = 0; i < list.Length; ++i)
        {
            Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(list[i]), elementStyle);
            bool hover = elementRect.Contains(Event.current.mousePosition);
            if (hover && Event.current.type == EventType.MouseDown)
            {
                selected = i;
                callback(i);
                Event.current.Use();
            }
            else if (Event.current.type == EventType.repaint)
            {
                elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
            }
        }
        return selected;
    }

}