public class Script : ScriptBase
{
    // private readonly List<List<string>> entries = new List<List<string>>();
    // private string currentEntry = "";
    // private bool insideQuotation = false;

    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        // HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
        // var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);

        // this.Context.Logger.LogDebug($"Parsing headers: {this.Context.Request.Headers}");
        // bool trimEntries = true;
        // bool skipBlankEntries = true;

        // string separator = (string)this.Context.Request.Headers.GetValues("x-separator").FirstOrDefault();
        // if (string.IsNullOrEmpty(separator)) {
        //     separator = ",";
        // }

        // bool.TryParse(GetHeaderString(this.Context.Request.Headers, "x-trim-whitespace"), out trimEntries);
        // bool.TryParse(GetHeaderString(this.Context.Request.Headers, "x-skip-blank-entries"), out skipBlankEntries);

        // this.Context.Logger.LogDebug($"trim: {trimEntries}, skip: {skipBlankEntries}, separator: {separator}");
        // response.Content = CreateJsonContent(ConvertCsvToJsonObject(contentAsString, trimEntries, skipBlankEntries, separator));
        // return response;

        Context.Logger.LogInformation($"started {DateTime.UtcNow}");

        switch (Context.OperationId)
        {
            case "Test":
                return await Test().ConfigureAwait(false);
                break;
            case "CreateZip":
                return await CreateZip().ConfigureAwait(false);
                break;
            // case "ExtractZip":
            //     return await ExtractZip().ConfigureAwait(false);
            //     break;
            default:
                break;
        }
        // Handle an invalid operation ID
        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        response.Content = CreateJsonContent($"Unknown operation ID '{this.Context.OperationId}'");
        return response;

    }
    private async Task<HttpResponseMessage> Test()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);
        JArray textArray = JArray.Parse(contentAsString);

        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = CreateJsonContent(new JObject
        {
            ["data"] = textArray
        }.ToString());

        return response;

    }
    private async Task<HttpResponseMessage> CreateZip()
    {
        var contentAsString = await this.Context.Request.Content.ReadAsStringAsync().ConfigureAwait(false);

        string zipFileName = (string)this.Context.Request.Headers.GetValues("x-zip-file-name").FirstOrDefault();
        if (string.IsNullOrEmpty(zipFileName))
        {
            zipFileName = "output.zip";
        }

        HttpResponseMessage response;

        JArray textArray = JArray.Parse(contentAsString);
        JArray arrayForZipping = new JArray();
        foreach (JObject item in textArray)
        {
            string filename;
            string content;
            if ( item.ContainsKey("filename") && item.ContainsKey("content") ) {
                filename = item.GetValue("filename").ToString();
                content = item.GetValue("content").ToString();
                // Console.WriteLine($"{name} == [{content}]");
                JObject fileObject = new JObject(
                    new JProperty("filename", filename),
                    new JProperty("content", content)
                );
                arrayForZipping.Add(fileObject);
            }
        }
        if (arrayForZipping.Count>0)
        {
            byte[] zipBytes = createZipFromArray(arrayForZipping).ToArray();
            response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(zipBytes);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = zipFileName
            };
            return response;
        }

        // Handle an invalid operation ID
        response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        response.Content = CreateJsonContent($"Unknown operation ID '{this.Context.OperationId}'");
        return response;


    }//end of CreateZip

    MemoryStream createZipFromArray(JArray fileArray)
    {

        using var outStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(outStream, System.IO.Compression.ZipArchiveMode.Create, true))
        {
            foreach (JObject item in fileArray)
            {
                var fileInArchive = archive.CreateEntry(item.GetValue("filename").ToString(), System.IO.Compression.CompressionLevel.Optimal);
                using var entryStream = fileInArchive.Open();
                // using var fileToCompressStream = new MemoryStream((byte[])item.GetValue("contents"));
                using var fileToCompressStream = new MemoryStream(Convert.FromBase64String(item.GetValue("content").ToString()));
                fileToCompressStream.CopyTo(entryStream);
            }
        }
        return outStream;
    }//--end of createZipFromArray

    // private async Task<HttpResponseMessage> ExtractZip()
    // {
    // }

    // private string ConvertCsvToJsonObject(string csvText, bool trimEntries, bool skipBlankEntries, string separator)
    // {
    //     var lines = csvText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    //     // var properties = lines[0].Split(',');
    //     char localSep = separator.ToCharArray()[0];
    //     var properties = lines[0].Split(localSep);
    //     for (int j = 0; j < properties.Length; j++) {
    //         properties[j] = properties[j].Trim();
    //     }

    //     foreach (string line in lines.Skip(1)) {
    //         if(insideQuotation || !string.IsNullOrWhiteSpace(line)) {
    //             // ScanNextLine(line);
    //             ScanNextLine(line, separator);
    //         }
    //     }

    //     var listObjResult = new List<Dictionary<string, string>>();

    //     foreach(var entry in entries) {
    //         var emptyLine = true;
    //         var objResult = new Dictionary<string, string>();
    //         for (int j = 0; j < properties.Length; j++) {
    //             var value = entry[j];

    //             objResult.Add(properties[j], value);
    //             if(!string.IsNullOrWhiteSpace(value)) {
    //                 emptyLine = false;
    //             }
    //         }

    //         if(!emptyLine || !skipBlankEntries) {
    //             listObjResult.Add(objResult);
    //         }
    //     }

    //     return JsonConvert.SerializeObject(listObjResult);
    // }

    // private void ScanNextLine(string line, string separator)
    // {
    //     char localSep = separator.ToCharArray()[0];
    //     // At the beginning of the line
    //     if (!insideQuotation)
    //     {
    //         entries.Add(new List<string>());
    //     }

    //     // The characters of the line
    //     foreach (char c in line)
    //     {
    //         if (insideQuotation)
    //         {
    //             if (c == '"')
    //             {
    //                 insideQuotation = false;
    //             }
    //             else
    //             {
    //                 currentEntry += c;
    //             }
    //         }
    //         // else if (c == ',')
    //         else if (c == localSep)
    //         {
    //             entries[entries.Count - 1].Add(currentEntry);
    //             currentEntry = "";
    //         }
    //         else if (c == '"')
    //         {
    //             insideQuotation = true;
    //         }
    //         else
    //         {
    //             currentEntry += c;
    //         }
    //     }

    //     // At the end of the line
    //     if (!insideQuotation)
    //     {
    //         entries[entries.Count - 1].Add(currentEntry);
    //         currentEntry = "";
    //     }
    //     else
    //     {
    //         currentEntry += "\n";
    //     }
    // }

    // private static string GetHeaderString(HttpHeaders headers, string name)
    // {
    //         IEnumerable<string> values;

    //         if (headers.TryGetValues(name, out values))
    //         {
    //             return values.FirstOrDefault();
    //         }

    //         return null;
    // }
}
