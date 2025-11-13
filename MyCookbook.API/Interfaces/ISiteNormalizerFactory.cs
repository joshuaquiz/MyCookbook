namespace MyCookbook.API.Interfaces;

public interface ISiteNormalizerFactory
{
    public ISiteNormalizer GetSiteNormalizer(
        string host);
}