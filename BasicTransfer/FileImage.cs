using System;
using System.IO;

namespace Basic_Transfer
{
    [Serializable]
    class FileImage : IDisposable
    {
        // Attributes
        public string Name { get; set; }
        public byte[] Data { get; set; }

        // Constructor
        public FileImage(string pathToFile)
        {
            // Get file attributes
            Name = Path.GetFileName(pathToFile);
            Data = File.ReadAllBytes(pathToFile); 
        }

        public void CreateFile(string path)
        {
            try
            {
                // Create file at specified path
                path = Path.Combine(path.Replace("\"", ""), Name);
                LoadingSpinner.Start();
                File.WriteAllBytes(path, Data);
                LoadingSpinner.Stop();
                Console.WriteLine("file created at [{0}]", path);
            }
            catch (Exception e)
            {
                LoadingSpinner.Stop();
                Program.Error(e.GetType().ToString() + " - " + e.Message, false);
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
                this.Data = null;

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
