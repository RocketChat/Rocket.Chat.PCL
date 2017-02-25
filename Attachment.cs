using System;
using Newtonsoft.Json.Linq;

namespace RocketChatPCL
{
	/// <summary>
	/// Represents an object attached to a message.
	/// </summary>
	public class Attachment
	{
		/// <summary>
		/// The title of the attachment.
		/// </summary>
		/// <value>The title.</value>
		public string Title { get; set; }
		/// <summary>
		/// The description of the attachment.
		/// </summary>
		/// <value>The description.</value>
		public string Description { get; set; }
		/// <summary>
		/// The link
		/// </summary>
		/// <value>The title link.</value>
		public string TitleLink { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether this should be downloaded.
		/// </summary>
		/// <value><c>true</c> if title link download; otherwise, <c>false</c>.</value>
		public bool TitleLinkDownload { get; set; }
		/// <summary>
		/// The image url of the attachment
		/// </summary>
		/// <value>The image URL.</value>
		public string ImageUrl { get; set; }
		/// <summary>
		/// The image type of the attachment
		/// </summary>
		/// <value>The type of the image.</value>
		public string ImageType { get; set; }
		/// <summary>
		/// The image size in bytes.
		/// </summary>
		/// <value>The size of the image.</value>
		public int ImageSize { get; set; }
		/// <summary>
		/// The image width in pixels.
		/// </summary>
		/// <value>The width of the image.</value>
		public int ImageWidth { get; set; }
		/// <summary>
		/// The image height in pixels.
		/// </summary>
		/// <value>The height of the image.</value>
		public int ImageHeight { get; set; }
		/// <summary>
		/// The Hex code of the colour
		/// </summary>
		/// <value>The colour.</value>
		public string Colour { get; set; }

		/// <summary>
		/// Parse the specified JSON obejct into an attachment
		/// </summary>
		/// <returns>An attachment.</returns>
		/// <param name="m">The JSON object</param>
		public static Attachment Parse(JObject m)
		{
			Attachment attach = new Attachment();

			if (m["title"] != null)
				attach.Title = m["title"].Value<string>();

			if (m["description"] != null)
				attach.Description = m["description"].Value<string>();

			if (m["titleLink"] != null)
				attach.TitleLink = m["title_link"].Value<string>();

			if (m["titleLinkDownload"] != null)
				attach.TitleLinkDownload = m["title_link_download"].Value<bool>();

			if (m["image_url"] != null)
				attach.ImageUrl = m["image_url"].Value<string>();

			if (m["image_type"] != null)
				attach.ImageType = m["image_type"].Value<string>();

			if (m["image_size"] != null)
				attach.ImageSize = m["image_size"].Value<int>();

			if (m["image_dimensions"] != null)
			{
				if (m["image_dimensions"]["width"] != null)
					attach.ImageWidth = m["image_dimensions"]["width"].Value<int>();
				
				if (m["image_dimensions"]["height"] != null)
					attach.ImageHeight = m["image_dimensions"]["height"].Value<int>();
			}

			return attach;
		}
	}
}
