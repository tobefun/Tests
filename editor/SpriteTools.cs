using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using UnityEditor.Sprites;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JJYDeZhou;

class SpriteTools : EditorWindow
{
    static int pageCount = 0;
    static int pageNum = 30;
    Vector2 scrollValue = Vector2.zero;
    static string unuseTag = "unuse";
    static Dictionary<string, Item> guid2Pics = new Dictionary<string, Item>();

    [MenuItem("Tools/DzpkTools/SpriteTools/图集引用工具")]
    static void OpenSpriteTools()
    {
        pageCount = 0;
        pageNum = 30;
        guid2Pics.Clear();
        EditorWindow.GetWindow<SpriteTools>();
    }

    void OnGUI()
    {
        List<string> keys = new List<string>(guid2Pics.Keys);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("读取引用文件"))
        {
            LoadRefrenceFile();
        }
        if (GUILayout.Button("生成引用关系"))
        {
            BuildRefrence();
        }

        if (GUILayout.Button("保存引用文件"))
        {
            SaveRefrence();
        }
        if (GUILayout.Button("设置packing tag"))
        {
            SetPackingTag(keys);
        }
        if (GUILayout.Button("移动到文件夹"))
        {
            MoveToFolders(keys);
        }
        EditorGUILayout.EndHorizontal();

#if UNITY_IPHONE
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("读取所有图片"))
        {
            ReadAllImageFiles();
        }
        if (GUILayout.Button("修改图片，并生成MD5名字"))
        {
            MoveAndRename(keys);
        }

        EditorGUILayout.EndHorizontal();
#endif
        EditorGUILayout.LabelField("注意：请以文件夹为单位进行选择！！！文件夹名即为包名！！！");

        scrollValue = EditorGUILayout.BeginScrollView(scrollValue);

        int startIndex = 0;
        for (int i = pageCount * pageNum; i < Mathf.Min((pageCount + 1) * pageNum, keys.Count); i++)
        {
            Item temp = guid2Pics[keys[i]];

            startIndex++;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(startIndex.ToString());

            FileInfo f = new FileInfo(temp.filePath);

            if (GUILayout.Button(f.Name, GUILayout.Width(300f)))
            {
                Debug.Log(f.Name, AssetDatabase.LoadAssetAtPath<Object>(FindRefrence.GetRelativeAssetsPath(temp.filePath)));
            }

            EditorGUILayout.LabelField(temp.refCount + "");

            EditorGUILayout.LabelField("CurTag: " + temp.CurTag);
            EditorGUILayout.LabelField("SetTag: " + temp.CreateTag);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        int count = keys.Count;
        string[] s = new string[count];
        int[] ints = new int[count];
        for (int i = 0; i < count / pageNum; i++)
        {
            s[i] = i.ToString();
            ints[i] = i;
        }
        pageCount = EditorGUILayout.IntPopup("CurPage", pageCount, s, ints, GUILayout.Width(200f));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    void ReadAllImageFiles()
    {
        List<string> picExt = new List<string>() { ".png", ".jpg" };

        string path = EditorUtility.OpenFolderPanel("选择要检索的文件夹", Application.dataPath + "/JJYDeZhou/BuildResource/Atlas", "");

        string[] allPics = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(s => picExt.Contains(Path.GetExtension(s).ToLower())).ToArray();

        for (int i = 0; i < allPics.Length; i++)
        {
            string guid = AssetDatabase.AssetPathToGUID(FindRefrence.GetRelativeAssetsPath(allPics[i]));
            if (guid2Pics.ContainsKey(guid))
            {
                Debug.LogError(guid + " allPics[i]:" + allPics[i]);
            }
            else
            {
                Item temp = new Item();
                temp.guid = guid;
                temp.filePath = allPics[i];
                if (guid2Pics.ContainsKey(temp.guid))
                {
                    guid2Pics[temp.guid] = temp;
                }
                else
                {
                    guid2Pics.Add(temp.guid, temp);
                }
            }
        }

        this.Repaint();
    }
    void BuildRefrence()
    {
        List<string> picExt = new List<string>() { ".png", ".jpg" };

        string path = EditorUtility.OpenFolderPanel("选择要检索的文件夹", Application.dataPath + "/JJYDeZhou/BuildResource/Atlas", "");

        string[] allPics = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(s => picExt.Contains(Path.GetExtension(s).ToLower())).ToArray();

        for (int i = 0; i < allPics.Length; i++)
        {
            string guid = AssetDatabase.AssetPathToGUID(FindRefrence.GetRelativeAssetsPath(allPics[i]));
            if (guid2Pics.ContainsKey(guid))
            {
                Debug.LogError(guid + " allPics[i]:" + allPics[i]);
            }
            else
            {
                Item temp = new Item();
                temp.guid = guid;
                temp.filePath = allPics[i];
                if (guid2Pics.ContainsKey(temp.guid))
                {
                    guid2Pics[temp.guid] = temp;
                }
                else
                {
                    guid2Pics.Add(temp.guid, temp);
                }
            }
        }


        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;

        EditorApplication.update = delegate ()
        {
            string file = files[startIndex];

            bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

            foreach (string guid in guid2Pics.Keys)
            {
                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    if (guid2Pics.ContainsKey(guid))
                    {
                        guid2Pics[guid].refCount++;
                    }
                    else
                    {
                        guid2Pics[guid].refCount = 1;
                    }

                    guid2Pics[guid].refFiles.Add(file);
                }
            }

            startIndex++;
            if (isCancel || startIndex >= files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                Debug.Log("匹配结束");

                this.Repaint();
            }
        };
    }

    void LoadRefrenceFile()
    {
        string path = EditorUtility.OpenFilePanel("选择保存的Refrence文件", Application.dataPath, "txt");
        string content = File.ReadAllText(path);
        string[] contents = content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < contents.Length; i++)
        {
            Item temp = new Item();
            temp.SetData(contents[i]);

            if (guid2Pics.ContainsKey(temp.guid))
            {
                guid2Pics[temp.guid] = temp;
            }
            else
            {
                guid2Pics.Add(temp.guid, temp);
            }
        }

    }

    void SaveRefrence()
    {
        string path = EditorUtility.SaveFilePanel("请保存引用关系结果", Application.dataPath, "fileRefrence", "txt");

        StringBuilder sb = new StringBuilder();
        foreach (string guid in guid2Pics.Keys)
        {
            sb.AppendLine(guid2Pics[guid].ToString());
        }
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();
    }

    void SetPackingTag(List<string> keys)
    {
        int index = 0;
        TextureImporter t = null;
        EditorApplication.update = delegate ()
        {
            Item item = guid2Pics[keys[index]];
            string file = item.filePath;

            file = FindRefrence.GetRelativeAssetsPath(file);
            bool isCancel = EditorUtility.DisplayCancelableProgressBar
            ("设置packing tag", file, (float)index / (float)keys.Count);

            t = AssetImporter.GetAtPath(file) as TextureImporter;

            if (t != null && (item.CurTag != item.CreateTag))
            {
                //非Texture模式
                t.spritePackingTag = item.CreateTag;
                t.SaveAndReimport();
            }
            index++;
            if (isCancel || index >= keys.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                index = 0;
                Debug.Log("匹配结束");

                //清空当前的tag，让它自己再读一遍
                this.Repaint();
            }
        };
    }

    void MoveToFolders(List<string> keys)
    {
        FileInfo f;
        int index = 0;
        EditorApplication.update = delegate ()
        {
            Item item = guid2Pics[keys[index]];
            string file = item.filePath;
            f = new FileInfo(file);

            string dicName = f.Directory.Name;

            bool isCancel = EditorUtility.DisplayCancelableProgressBar
            ("开始移动文件", file, (float)index / (float)keys.Count);
            index++;

            if (isCancel || index >= keys.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                index = 0;
                Debug.Log("移动文件结束");

                this.Repaint();

                SaveRefrence();
            }

            if (f.Exists)
            {
                if (dicName != "Atlas")
                {
                    if (dicName != item.CreateTag)
                    {
                        string repFile = file.Replace(dicName, item.CurTag);
                        f = new FileInfo(repFile);

                        if (!Directory.Exists(f.DirectoryName))
                        {
                            Directory.CreateDirectory(f.DirectoryName);
                        }

                        File.Move(file, repFile);

                        File.Move(file + ".meta", repFile + ".meta");

                        item.filePath = repFile;

                        item.CurTag = "";
                    }
                }
            }
            
        };
    }

    void MoveAndRename(List<string> keys)
    {
        FileInfo f;
        int index = 0;
        EditorApplication.update = delegate ()
        {
            Item item = guid2Pics[keys[index]];
            string file = item.filePath;
            f = new FileInfo(file);

            string dicName = f.Directory.Name;

            bool isCancel = EditorUtility.DisplayCancelableProgressBar
            ("开始修改图片", file, (float)index / (float)keys.Count);
            index++;

            if (f.Exists)
            {
                string md51 = Util.md5file(file);

                ImageHelper.ChangeImg(file);

                string md5 = Util.md5file(file);

                Debug.Log(md51 + "     " + md5);
                file = Path.GetFullPath(file);
                string destFile = file.Substring(0, file.LastIndexOf(Util.GetPathSpliter()) + 1) + md5 + Path.GetExtension(file);

                File.Move(file, destFile);
                File.Move(file + ".meta", destFile + ".meta");

                item.filePath = destFile;
            }

            if (isCancel || index >= keys.Count)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                index = 0;
                Debug.Log("移动文件结束");

                this.Repaint();

                SaveRefrence();
            }
        };
    }
    // 索引的文件
    class Item
    {
        public string filePath;
        public string guid;
        public int refCount;
        public List<string> refFiles;

        string curTag;
        public string CurTag
        {
            get
            {
                if (curTag != null && curTag != "")
                {
                    return curTag;
                }

                TextureImporter t = AssetImporter.GetAtPath(FindRefrence.GetRelativeAssetsPath(filePath)) as TextureImporter;

                if (t != null)
                    curTag = t.spritePackingTag;
                else
                    curTag = "";

                return curTag;
            }
            set
            {
                curTag = value;
            }
        }

        string createTag;
        public string CreateTag
        {
            get
            {
                if (createTag != null && createTag != "")
                {
                    return createTag;
                }

                if (refCount == 0)
                {
                    createTag = unuseTag;
                    return createTag;
                }

                List<string> slist = new List<string>();
                FileInfo f;
                for (int i = 0; i < refFiles.Count; i++)
                {
                    f = new FileInfo(refFiles[i]);

                    string name = f.Name.ToLower();

                    if (name.IndexOf("panel") != -1)
                    {
                        name = name.Substring(0, name.IndexOf("panel"));
                    }
                    else
                    {
                        if (name.IndexOf(".") != -1)
                        {
                            name = name.Substring(0, name.IndexOf("."));
                        }
                        else
                        {
                            Debug.Log(name);
                        }
                    }

                    name = name.Replace("dzpk", "");

                    slist.Add(name);
                }
                slist.Sort();

                string s = slist[0];
                for (int i = 1; i < slist.Count; i++)
                {
                    s += slist[i].Substring(0, 1);
                }

                createTag = s.ToLower();
                return createTag;

            }
        }

        public Item()
        {
            refFiles = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(guid);
            sb.Append("\t");
            sb.Append(filePath);
            sb.Append("\t");
            sb.Append(refCount + "");
            sb.Append("\t");
            foreach (string s in refFiles)
            {
                sb.Append(s);
                sb.Append("\t");
            }
            return sb.ToString();
        }

        public void SetData(string s)
        {
            string[] array = s.Split(new string[] { "\t" }, System.StringSplitOptions.RemoveEmptyEntries);
            guid = array[0];
            filePath = array[1];
            refCount = int.Parse(array[2]);
            refFiles.Clear();
            if (refCount > 0)
            {
                for (int i = 3; i < array.Length; i++)
                {
                    refFiles.Add(array[i]);
                }
            }
        }
    }
}

