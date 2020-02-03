using System;
using System.IO;
using System.Reflection;

namespace BF4_Private_By_Tejisav
{
    class Program
    {
        public static void Main(string[] args)
        {
            AssemblyResolver.Register();
            WithOverlay.StartHack();
        }

        // Load an embedded DLL via stream
        internal class AssemblyResolver
        {
            public static void Register()
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, arguments) =>
                {
                    string name = new AssemblyName(arguments.Name).Name;

                    if (name.Contains(".resources"))
                    {
                        return null;
                    }

                    string resource = typeof(WithOverlay).Namespace + ".Resources." + name + ".dll";

                    try
                    {
                        Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

                        if (stream != null)
                        {
                            using (stream)
                            {
                                byte[] bytes = new byte[stream.Length];
                                stream.Read(bytes, 0, bytes.Length);

                                //Console.WriteLine("Loaded: {0}", name);
                                return Assembly.Load(bytes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    return null;
                };
            }
        }
    }
}
