namespace IbanValidator
{
    sealed class Files
    {
        static IEnumerable<string> FetchRecursively(string path)
        {
            var queue = new Queue<string>();
            queue.Enqueue(path);

            while (queue.Count > 0)
            {
                path = queue.Dequeue();

                try
                {
                    foreach (var dir in Directory.GetDirectories(path))
                        queue.Enqueue(dir);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

                string[] files = {};

                try
                {
                    files = Directory.GetFiles(path);
                } 
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                
                if (files != null)
                    for (int i = 0; i < files.Length; i++)
                        yield return files[i];
            }
        }
    }
}