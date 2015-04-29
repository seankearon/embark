﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Embark.Storage
{
    internal class FileDataStore
    {
        public FileDataStore(string directory)
        {
            if (!directory.EndsWith("\\"))
                directory += "\\";

            var collectionsFolder = directory + @"Collections\";
            var keysFolder = directory + @"Map\";
            //var logfolder = directory + @"Pending\";
            
            this.keyProvider = new KeyProvider(keysFolder);
            this.tagPaths = new CollectionPaths(collectionsFolder);
        }
        
        private KeyProvider keyProvider;
        private CollectionPaths tagPaths;
        private object syncRoot = new object();

        public long Insert(string tag, string objectToInsert)
        {
            // Get ID from IDGen
            var key = keyProvider.GetNewKey();
                
            // TODO 3 offload to queue that gets processed by task
            var savePath = tagPaths.GetDocumentPath(tag, key.ToString());

            // TODO 1 NB get a document only lock, instead of all repositories lock
            lock (syncRoot)
            {
                // Save object to tag dir
                File.WriteAllText(savePath, objectToInsert);

                //Return ID to client
                return key;
            }
        }
        
        public bool Update(string tag, string id, string objectToUpdate)
        {
            var savePath = tagPaths.GetDocumentPath(tag, id);
            
            lock(syncRoot)
            {
                if (!File.Exists(savePath))
                    return false;
                else
                {
                    File.WriteAllText(savePath, objectToUpdate);
                    return true;
                }
            }
        }

        public bool Delete(string tag, string id)
        {
            var savePath = tagPaths.GetDocumentPath(tag, id);

            lock (syncRoot)
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    return true;
                }
                else return false;
            }
        }

        public string Get(string tag, string id)
        {
            var savePath = tagPaths.GetDocumentPath(tag, id);

            string jsonText;

            // TODO lock row only
            lock (syncRoot)
            {
                if (!File.Exists(savePath))
                    return null;

                jsonText = File.ReadAllText(savePath);
            }
            return jsonText;
        }

        public DataEnvelope[] GetAll(string tag)
        {
            lock (syncRoot)
            {
                var tagDir = tagPaths.GetCollectionDirectory(tag);

                var allFiles = Directory
                    .GetFiles(tagDir)
                    //.EnumerateFiles(tagDir)
                    .Select(filePath => GetDataEnvelope(filePath))
                    .ToArray();

                return allFiles;
            }
        }

        // TODO Try/Catch return error envelope.
        private DataEnvelope GetDataEnvelope(string filePath)
        {
            return new DataEnvelope
            {
                ID = Int64.Parse(Path.GetFileNameWithoutExtension(filePath)),
                Text = File.ReadAllText(filePath)
            };
        }
    }
}