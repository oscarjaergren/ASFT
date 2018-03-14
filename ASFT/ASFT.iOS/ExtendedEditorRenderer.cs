using ASFT.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedEditorRenderer), typeof(ExtendedEditorRenderer))]
namespace ASFT.iOS
{
    public class ExtendedEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {

            base.OnElementChanged(e);


            if (Control != null)
            {
                Control.Layer.BorderColor = UIColor.Gray.CGColor;
                Control.Layer.BorderWidth = 1;
                Control.Layer.CornerRadius = 0.5f;

            }

        }

    }
}
