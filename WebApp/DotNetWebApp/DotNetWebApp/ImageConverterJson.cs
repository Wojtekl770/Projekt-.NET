using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetWebApp
{

	public class Base64FileConverter 
	{
		public byte [] UploadDocument(IFormFile file)
		{

			long length = file.Length;
			if (length < 0)
				return [];

			using var fileStream = file.OpenReadStream();
			byte[] bytes = new byte[length];
			fileStream.Read(bytes, 0, (int)file.Length);

			return bytes;
		}
	}
}
