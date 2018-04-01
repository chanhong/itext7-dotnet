using System;
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.StyledXmlParser.Css.Util;
using iText.Svg;
using iText.Svg.Renderers;
using iText.Svg.Utils;

namespace iText.Svg.Renderers.Impl {
    /// <summary>
    /// Abstract class that will be the superclass for any element that can function
    /// as a parent.
    /// </summary>
    public abstract class AbstractBranchSvgNodeRenderer : AbstractSvgNodeRenderer {
        /// <summary>
        /// Method that will set properties to be inherited by this branch renderer's
        /// children and will iterate over all children in order to draw them.
        /// </summary>
        /// <param name="context">
        /// the object that knows the place to draw this element and
        /// maintains its state
        /// </param>
        protected internal override void DoDraw(SvgDrawContext context) {
            if (GetChildren().Count > 0) {
                // if branch has no children, don't do anything
                PdfStream stream = new PdfStream();
                stream.Put(PdfName.Type, PdfName.XObject);
                stream.Put(PdfName.Subtype, PdfName.Form);
                stream.Put(PdfName.BBox, new PdfArray(context.GetCurrentViewPort()));
                PdfFormXObject xObject = (PdfFormXObject)PdfXObject.MakeXObject(stream);
                PdfCanvas newCanvas = new PdfCanvas(xObject, context.GetCurrentCanvas().GetDocument());
                ApplyViewBox(context);
                context.PushCanvas(newCanvas);
                ApplyViewport(context);
                foreach (ISvgNodeRenderer child in GetChildren()) {
                    newCanvas.SaveState();
                    child.Draw(context);
                    newCanvas.RestoreState();
                }
                CleanUp(context);
                AffineTransform transformation = new AffineTransform();
                if (attributesAndStyles != null && attributesAndStyles.ContainsKey(SvgTagConstants.TRANSFORM)) {
                    transformation = TransformUtils.ParseTransform(attributesAndStyles.Get(SvgTagConstants.TRANSFORM));
                }
                // TODO DEVSIX-1891
                float[] matrixValues = new float[6];
                transformation.GetMatrix(matrixValues);
                // TODO DEVSIX-1890
                context.GetCurrentCanvas().AddXObject(xObject, matrixValues[0], matrixValues[1], matrixValues[2], matrixValues
                    [3], matrixValues[4], matrixValues[5]);
                if (attributesAndStyles != null && attributesAndStyles.ContainsKey(SvgTagConstants.ID)) {
                    context.AddNamedObject(attributesAndStyles.Get(SvgTagConstants.ID), xObject);
                }
            }
        }

        /// <summary>Applies a transformation based on a viewBox for a given branch node.</summary>
        /// <param name="context">current svg draw context</param>
        private void ApplyViewBox(SvgDrawContext context) {
            if (this.attributesAndStyles != null) {
                if (this.attributesAndStyles.ContainsKey(SvgTagConstants.VIEWBOX)) {
                    String viewBoxValues = attributesAndStyles.Get(SvgTagConstants.VIEWBOX);
                    IList<String> valueStrings = SvgCssUtils.SplitValueList(viewBoxValues);
                    float[] values = new float[valueStrings.Count];
                    for (int i = 0; i < values.Length; i++) {
                        values[i] = CssUtils.ParseAbsoluteLength(valueStrings[i]);
                    }
                    Rectangle currentViewPort = context.GetCurrentViewPort();
                    float scaleWidth = currentViewPort.GetWidth() / values[2];
                    float scaleHeight = currentViewPort.GetHeight() / values[3];
                    AffineTransform scale = AffineTransform.GetScaleInstance(scaleWidth, scaleHeight);
                    context.GetCurrentCanvas().ConcatMatrix(scale);
                    AffineTransform transform = ProcessAspectRatio(context, values);
                    context.GetCurrentCanvas().ConcatMatrix(transform);
                }
            }
        }

        /// <summary>Applies a clipping operation based on the view port.</summary>
        /// <param name="context">the svg draw context</param>
        private void ApplyViewport(SvgDrawContext context) {
            if (GetParent() != null) {
                PdfCanvas currentCanvas = context.GetCurrentCanvas();
                currentCanvas.Rectangle(context.GetCurrentViewPort());
                currentCanvas.Clip();
                currentCanvas.NewPath();
            }
        }

        /// <summary>If present, process the preserveAspectRatio.</summary>
        /// <param name="context">the svg draw context</param>
        /// <param name="viewBoxValues">the four values depicting the viewbox [min-x min-y width height]</param>
        /// <returns>the transformation based on the preserveAspectRatio value</returns>
        private AffineTransform ProcessAspectRatio(SvgDrawContext context, float[] viewBoxValues) {
            AffineTransform transform = new AffineTransform();
            if (this.attributesAndStyles.ContainsKey(SvgTagConstants.PRESERVE_ASPECT_RATIO)) {
                Rectangle currentViewPort = context.GetCurrentViewPort();
                String preserveAspectRatioValue = this.attributesAndStyles.Get(SvgTagConstants.PRESERVE_ASPECT_RATIO);
                IList<String> values = SvgCssUtils.SplitValueList(preserveAspectRatioValue);
                if (SvgTagConstants.DEFER.EqualsIgnoreCase(values[0])) {
                    values.JRemoveAt(0);
                }
                String align = values[0];
                float x = 0f;
                float y = 0f;
                float midXBox = viewBoxValues[0] + (viewBoxValues[2] / 2);
                float midYBox = viewBoxValues[1] + (viewBoxValues[3] / 2);
                float midXPort = currentViewPort.GetX() + (currentViewPort.GetWidth() / 2);
                float midYPort = currentViewPort.GetY() + (currentViewPort.GetHeight() / 2);
                switch (align.ToLowerInvariant()) {
                    case SvgTagConstants.NONE: {
                        break;
                    }

                    case SvgTagConstants.XMIN_YMIN: {
                        x = -viewBoxValues[0];
                        y = -viewBoxValues[1];
                        break;
                    }

                    case SvgTagConstants.XMIN_YMID: {
                        x = -viewBoxValues[0];
                        y = midYPort - midYBox;
                        break;
                    }

                    case SvgTagConstants.XMIN_YMAX: {
                        x = -viewBoxValues[0];
                        y = currentViewPort.GetHeight() - viewBoxValues[3];
                        break;
                    }

                    case SvgTagConstants.XMID_YMIN: {
                        x = midXPort - midXBox;
                        y = -viewBoxValues[1];
                        break;
                    }

                    case SvgTagConstants.XMID_YMAX: {
                        x = midXPort - midXBox;
                        y = currentViewPort.GetHeight() - viewBoxValues[3];
                        break;
                    }

                    case SvgTagConstants.XMAX_YMIN: {
                        x = currentViewPort.GetWidth() - viewBoxValues[2];
                        y = -viewBoxValues[1];
                        break;
                    }

                    case SvgTagConstants.XMAX_YMID: {
                        x = currentViewPort.GetWidth() - viewBoxValues[2];
                        y = midYPort - midYBox;
                        break;
                    }

                    case SvgTagConstants.XMAX_YMAX: {
                        x = currentViewPort.GetWidth() - viewBoxValues[2];
                        y = currentViewPort.GetHeight() - viewBoxValues[3];
                        break;
                    }

                    case SvgTagConstants.DEFAULT_ASPECT_RATIO:
                    default: {
                        x = midXPort - midXBox;
                        y = midYPort - midYBox;
                        break;
                    }
                }
                transform.Translate(x, y);
            }
            return transform;
        }

        /// <summary>Cleans up the SvgDrawContext by removing the current viewport and by popping the current canvas.</summary>
        /// <param name="context">context to clean</param>
        private void CleanUp(SvgDrawContext context) {
            if (GetParent() != null) {
                context.RemoveCurrentViewPort();
            }
            context.PopCanvas();
        }
    }
}
