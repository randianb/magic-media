using HotChocolate.Types;
using MagicMedia.Extensions;
using MagicMedia.Store;

namespace MagicMedia.Api.GraphQL
{
    public class ThumbnailType : ObjectType<MediaThumbnail>
    {
        protected override void Configure(IObjectTypeDescriptor<MediaThumbnail> descriptor)
        {
            descriptor.Ignore(x => x.Data);

            descriptor
                .Field("dataUrl")
                .Type(typeof(string))
                .ResolveWith<ThumbnailResolvers>(x => x.GetDataUrl(default!, default!));
        }
    }
}
