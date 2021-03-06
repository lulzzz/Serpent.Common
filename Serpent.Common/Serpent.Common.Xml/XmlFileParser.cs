﻿namespace Serpent.Common.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml;

    using Serpent.Common.BaseTypeExtensions.Collections;

    public class XmlFileParser
    {
        public async Task ReadXmlFileAsync(string filename, Func<string, XmlTextReader, bool> elementHandlerFunc, Func<string, bool> endElementHandlerFunc = null)
        {
            using (var dataStream = await GetDataStreamAsync(filename))
            {
                this.ReadXmlStream(dataStream, elementHandlerFunc, endElementHandlerFunc);
            }
        }

        public void ReadXmlStream(Stream dataStream, Func<string, XmlTextReader, bool> elementHandlerFunc, Func<string, bool> endElementHandlerFunc = null)
        {
            var pathList = new List<string>(16)
                               {
                                   string.Empty
                               };

            using (var xmlReader = new XmlTextReader(dataStream))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        var currentPath = pathList.LastItem() + "/" + xmlReader.Name;

                        if (xmlReader.IsEmptyElement == false)
                        {
                            pathList.Add(currentPath);
                        }

                        if (!elementHandlerFunc(currentPath, xmlReader))
                        {
                            break;
                        }

                        if (xmlReader.IsEmptyElement)
                        {
                            endElementHandlerFunc?.Invoke(currentPath);
                        }
                    }

                    if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        endElementHandlerFunc?.Invoke(pathList.LastItem());
                        pathList.RemoveLast();
                    }
                }
            }
        }

        public async Task<Result<T>> ReadXmlFileAsync<T>(
            string filename,
            T item,
            IReadOnlyDictionary<string, Action<T, string>> elementMap,
            IReadOnlyDictionary<string, Action<T>> endElementMap = null,
            Func<string, XmlTextReader, bool> elementHandlerFunc = null)
        {
            using (var dataStream = await GetDataStreamAsync(filename))
            {
                return await this.ReadXmlFileAsync<T>(dataStream, filename, item, elementMap, endElementMap, elementHandlerFunc);
            }
        }

#pragma warning disable 1998
        public async Task<Result<T>> ReadXmlFileAsync<T>(
#pragma warning restore 1998
            Stream dataStream,
            string filename,
            T item,
            IReadOnlyDictionary<string, Action<T, string>> elementMap,
            IReadOnlyDictionary<string, Action<T>> endElementMap = null,
            Func<string, XmlTextReader, bool> elementHandlerFunc = null)
        {
            try
            {
                this.ReadXmlStream(
                    dataStream,
                    (path, reader) =>
                        {
                            if (reader.IsEmptyElement == false)
                            {
                                if (elementMap.TryGetValue(path, out var action))
                                {
                                    if (string.IsNullOrWhiteSpace(reader.Value) == false)
                                    {
                                        action(item, reader.Value);
                                    }
                                    else
                                    {
                                        action(item, reader.ReadContent());
                                    }
                                }
                            }

                            if (elementHandlerFunc != null)
                            {
                                return elementHandlerFunc(path, reader);
                            }

                            return true;
                        },
                    path =>
                        {
                            if (endElementMap != null)
                            {
                                if (endElementMap.TryGetValue(path, out var action))
                                {
                                    action(item);
                                }
                            }

                            return true;
                        });

                return new Result<T>(filename, ResultStatus.Success, item);
            }
            catch (Exception exception)
            {
                return new Result<T>(filename, ResultStatus.Failure, exception);
            }
        }

        private static async Task<Stream> GetDataStreamAsync(string filename)
        {
            Stream dataStream;

            using (var fileStream = File.OpenRead(filename))
            {
                var buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, (int)fileStream.Length);

                dataStream = new MemoryStream(buffer);
            }

            return dataStream;
        }
    }
}