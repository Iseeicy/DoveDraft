using System;
using System.Collections.Generic;
using Godot;

namespace DoveDraft.Editor;

[Tool, GlobalClass]
public partial class ResourceSearchList : ItemList
{
    //
    //  Exports
    //

    [Export]
    public ResourceSearchFilter Filter { get; set; }
    
    //
    //  Public Variables
    //

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (value == _searchQuery) return;
            _searchQuery = value;
            RefreshList();
        }
    }
    private string _searchQuery = "";
    
    //
    //  Private Variables
    //

    private ResourceSearchFilter _defaultFilter = new();
    private List<string> _knownFiles = new();
    
    //
    //  Public Methods
    //

    public void ScanFileSystem()
    {
        _knownFiles.Clear();
        ScanDirectory("res://", _knownFiles);
        RefreshList();
    }
    
    public void RefreshList()
    {
        Clear();
        foreach (var file in _knownFiles)
        {
            if (FileMatchesSearch(file, SearchQuery)) AddItem(file);
        }
    }
    
    //
    //  Private Methods
    //

    private void ScanDirectory(string path, List<string> results)
    {
        if (ShouldIgnoreDirectory(path)) return;

        DirAccess dir = DirAccess.Open(path);
        if (dir == null) return;

        dir.ListDirBegin();

        // Loop through every path in this directory
        for (var fileName = dir.GetNext(); !String.IsNullOrEmpty(fileName); fileName = dir.GetNext())
        {
            string fullPath;
            if (path == "res://")
            {
                fullPath = path + fileName;
            }
            else
            {
                fullPath = path + "/" + fileName;
            }

            if (dir.CurrentIsDir())
            {
                ScanDirectory(fullPath, results);
                continue;
            }
            
            if (ShouldIncludeFile(fullPath)) results.Add(fullPath);
        }
    }

    private bool ShouldIgnoreDirectory(string absolutePath)
    {
        if (absolutePath == "res://.godot") return true;
        return false;
    }

    private bool ShouldIncludeFile(string absolutePath)
    {
        if (!absolutePath.EndsWith(".tres")) return false;

        var foundResource = GD.Load<Resource>(absolutePath);
        if (foundResource == null) return false;

        var filterToUse = Filter ?? _defaultFilter;
        return filterToUse.ShouldResourceBeIncluded(absolutePath, foundResource);
    }

    private bool FileMatchesSearch(string absolutePath, string query)
    {
        return String.IsNullOrEmpty(query) || absolutePath.Contains(query);
    }
}