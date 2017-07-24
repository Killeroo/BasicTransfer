using System;
using System.IO;

namespace Basic_Transfer
{
    [Serializable]
    class FileImage : IDisposable
    {
        // Attributes
        public string name { get; set; }
        public byte[] data { get; set; }

        // Constructor
        public FileImage(string pathToFile)
        {
            // First check that file exists
            if (!System.IO.File.Exists(pathToFile))
                Program.Error("File cannot be found at location \"" + pathToFile + "\"");

            // Get file attributes
            name = Path.GetFileName(pathToFile);
            data = File.ReadAllBytes(pathToFile); 
        }

        public void createFile(string path)
        {
            try
            {
                // Create file at specified path
                path = Path.Combine(path.Replace("\"", ""), name);
                File.WriteAllBytes(path, data);
                Console.WriteLine("file created at [{0}]", path);
            }
            catch (DirectoryNotFoundException)
            {
                Program.Error("Cannot save file, \"" + path + "\" not found.");
            }
            catch (IOException)
            {
                Program.Error("Cannot write file to device");
            }
            catch (UnauthorizedAccessException)
            {
                Program.Error("Access Denied. Can't write to directory");
            }
            catch (Exception e)
            {
                Program.Error("Error occured creating file "  + "Error msg: " + e.InnerException + " " + e.Message);
            }
        }

        #region IDisposable Support http://stackoverflow.com/questions/538060/proper-use-of-the-idisposable-interface
        private bool disposedValue = false; // To detect redundant dispose calls

        // Interface to IDispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

                // Set File data field to null
                this.data = null;

                // Call garbage collector to free up memory
                System.GC.Collect();

                // Stop redundant calls
                disposedValue = true;
                
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
