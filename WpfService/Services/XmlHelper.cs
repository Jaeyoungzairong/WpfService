using System;
using System.IO;
using System.Xml.Serialization;

namespace WpfService.Services
{
    public class XmlHelper<T>
    {
        private Type _type;

        public XmlHelper()
        {
            _type = typeof(T);
        }

        public void Save(string path, object obj)
        {
            using (TextWriter textWriter = new StreamWriter(path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(_type);
                    serializer.Serialize(textWriter, obj);
                }
                catch (Exception ex)
                {
                    Dialog.Error(ex.ToString());
                }
                textWriter.Close();
            }
        }

        public T Read(string path)
        {
            T result = default(T);
            using (TextReader textReader = new StreamReader(path))
            {
                try
                {
                    XmlSerializer deserializer = new XmlSerializer(_type);
                    result = (T)deserializer.Deserialize(textReader);
                }
                catch (Exception ex)
                {
                    Dialog.Error(ex.ToString());
                }
                textReader.Close();
            }
            return result;
        }
    }
}
