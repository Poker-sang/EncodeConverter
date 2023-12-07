using System.Linq;
using System.Text;
using EncodeConverter.Misc;
using UtfUnknown;

namespace EncodeConverter.Pages;

public class TextPageViewModel : AbstractViewModel
{
    public void Detect()
    {
        var result = CharsetDetector.DetectFromBytes(_stringBytes);
        DetectionDetails = result.Details
            .Where(d => d.Confidence >= 0.5)
            .OrderByDescending(x => x.Confidence)
            .Select(d => EncodingHelper.GetEncodingItemOrFetch(d.Encoding.CodePage))
            .ToList();
        OnPropertyChanged(nameof(DetectionDetails));
    }

    private string _inputText = "";

    public string InputText
    {
        get => _inputText;
        set
        {
            if (value == _inputText)
                return;
            _inputText = value;
            var strEncoding = Encoding.GetEncoding(DestinationEncoding.CodePage);
            _stringBytes = strEncoding.GetBytes(_inputText);
            OnPropertyChanged();
            OnPropertyChanged(nameof(OutputText));
            OnPropertyChanged(nameof(StringBytes));
        }
    }

    private byte[] _stringBytes = [];

    public string StringBytes
    {
        get
        {
            var sb = _stringBytes.Aggregate(new StringBuilder(), (current, b) => current.Append(b.ToString("X2")).Append(' '));
            return sb.Length > 0 ? sb.ToString(0, sb.Length - 1) : "";
        }
    }

    public string OutputText
    {
        get
        {
            var newEncoding = Encoding.GetEncoding(OriginalEncoding.CodePage);
            return newEncoding.GetString(_stringBytes);
        }
    }

    protected override void OnOriginalEncodingChanged() => OnPropertyChanged(nameof(OutputText));

    protected override int SourceOriginalEncodingCodePage
    {
        get => AppSetting.TextOriginalEncodingCodePage;
        set
        {
            AppSetting.TextOriginalEncodingCodePage = value;
            var strEncoding = Encoding.GetEncoding(DestinationEncoding.CodePage);
            _stringBytes = strEncoding.GetBytes(InputText);
        }
    }

    protected override int SourceDestinationEncodingCodePage
    {
        get => AppSetting.TextDestinationEncodingCodePage;
        set
        {
            AppSetting.TextDestinationEncodingCodePage = value;
            OnPropertyChanged(nameof(StringBytes));
            OnPropertyChanged(nameof(OutputText));
        }
    }

    protected override bool SourceDestinationEncodingUseSystem
    {
        get => AppSetting.TextDestinationEncodingUseSystem;
        set => AppSetting.TextDestinationEncodingUseSystem = value;
    }

    //public bool DoNotTranscode
    //{
    //    get => AppSetting.TextDoNotTranscode;
    //    set
    //    {
    //        if (value == AppSetting.TextDoNotTranscode)
    //            return;
    //        AppSetting.TextDoNotTranscode = value;
    //        AppContext.SaveConfiguration(AppSetting);
    //        OnPropertyChanged();
    //    }
    //}
}
