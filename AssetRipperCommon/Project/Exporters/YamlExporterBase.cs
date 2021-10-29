using AssetRipper.Core.IO;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Core.Project.Exporters
{
	public abstract class YamlExporterBase : IAssetExporter
	{
		public virtual bool IsHandle(UnityObjectBase asset)
		{
			return true;
		}

		public bool Export(IExportContainer container, UnityObjectBase asset, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (InvariantStreamWriter streamWriter = new InvariantStreamWriter(stream, UTF8))
					{
						YAMLWriter writer = new YAMLWriter();
						YAMLDocument doc = asset.ExportYAMLDocument(container);
						writer.AddDocument(doc);
						writer.Write(streamWriter);
					}
				}
			}
			return true;
		}

		public void Export(IExportContainer container, UnityObjectBase asset, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path)
		{
			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				using (BufferedStream stream = new BufferedStream(fileStream))
				{
					using (InvariantStreamWriter streamWriter = new InvariantStreamWriter(stream, UTF8))
					{
						YAMLWriter writer = new YAMLWriter();
						writer.WriteHead(streamWriter);
						foreach (UnityObjectBase asset in assets)
						{
							YAMLDocument doc = asset.ExportYAMLDocument(container);
							writer.WriteDocument(doc);
						}
						writer.WriteTail(streamWriter);
					}
				}
			}
			return true;
		}

		public void Export(IExportContainer container, IEnumerable<UnityObjectBase> assets, string path, Action<IExportContainer, UnityObjectBase, string> callback)
		{
			throw new NotSupportedException("YAML supports only single file export");
		}

		public abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset);

		public AssetType ToExportType(UnityObjectBase asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Serialized;
			return true;
		}

		private static readonly Encoding UTF8 = new UTF8Encoding(false);
	}
}