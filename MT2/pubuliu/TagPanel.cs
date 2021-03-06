﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MT2.pubuliu
{
    class TagPanel:Panel
    {
        //来自http://blog.higan.me/uwp-control-development-02/
        //指定一个流的数量
        public int StatckCount
        {
            get { return (int)GetValue(StatckCountProperty); }
            set { SetValue(StatckCountProperty, value); }
        }

        public static readonly DependencyProperty StatckCountProperty =
                DependencyProperty.Register("StatckCount", typeof(int), typeof(VirtualizingPanel), new PropertyMetadata(1));
        //控制瀑布流的方向
        public Orientation Orientation
        {
            get { return (Orientation)GetValue (OrientationProperty ) ; }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VirtualizingPanel), new PropertyMetadata(Orientation.Vertical));
        //控制间距
        public Double ItemsSpacing
        {
            get { return (Double)GetValue(ItemsSpacingProperty); }
            set { SetValue(ItemsSpacingProperty, value); }
        }

        public static readonly DependencyProperty ItemsSpacingProperty =
                DependencyProperty.Register("ItemsSpacing", typeof(Double), typeof(VirtualizingPanel), new PropertyMetadata(10));
 
      public Double StatckSpacing
        {
            get { return (Double)GetValue(StatckSpacingProperty); }
            set { SetValue(StatckSpacingProperty, value); }
        }

        public static readonly DependencyProperty StatckSpacingProperty =
                DependencyProperty.Register("StatckSpacing", typeof(Double), typeof(VirtualizingPanel), new PropertyMetadata(10));

        //用于重新测量和排列布局
        //private  static async void RequestArrange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    try
        //    {
        //        (d as VirtualizingPanel).InvalidateMeasure();
        //        (d as VirtualizingPanel).InvalidateArrange();
        //    }
        // catch(Exception ex)
        //    {
        //     await  new MessageDialog(ex.ToString()).ShowAsync();
        //    }
        //}

        #region 测量 
        protected override Size MeasureOverride(Size availableSize)
        {
            var measure = base.MeasureOverride(availableSize);

            double itemFixed = 0;
            Size requestSize = Size.Empty;

            //判断面板的布局类型,是横向布局还是纵向布局
            if (Orientation == Orientation.Vertical)
            {
                //纵向布局

                //创建一个列表记录所有 Stack 的长度
                List<Double> offsetY = new Double[StatckCount].ToList();

                //计算一个 Item 的固定边长度,纵向布局的话是宽固定
                itemFixed = (availableSize.Width - StatckSpacing * (StatckCount - 1)) / StatckCount;

                requestSize = new Size()
                {
                    //设定需要的空间的宽,一般是提供多少要多少
                    Width = availableSize.Width
                };

                //遍历 Children 来测量长度
                foreach (var item in this.Children)
                {
                    //寻找最短的 Stack ,将新的 Item 分配到这个 Stack
                    int minIndex = offsetY.IndexOf(offsetY.Min());
                    //向 Item 发送测量请求,让 Item 测量自己需要的空间
                    item.Measure(new Size(itemFixed, double.PositiveInfinity));
                    //测量结果保存在 DesiredSize 属性里面
                    var itemRequestSize = item.DesiredSize;
                    //将这个 Stack 的长度加上新的 Item 的长度和 Item 的间隙
                    offsetY[minIndex] += itemRequestSize.Height + ItemsSpacing;
                }
                //寻找最长的 Stack,这个 Stack 就是面板需要的高度
                requestSize.Height = offsetY.Max();
            }
            else
            {
                //横向布局,内容大同小异,区别就是把长变成了宽

                List<Double> offsetX = new Double[StatckCount].ToList();

                //Item 的固定边为长
                itemFixed = (availableSize.Height - StatckSpacing * (StatckCount - 1)) / StatckCount;

                requestSize = new Size()
                {
                    Height = availableSize.Height
                };

                foreach (var item in this.Children)
                {
                    int minIndex = offsetX.IndexOf(offsetX.Min());
                    item.Measure(new Size(double.PositiveInfinity, itemFixed));
                    var itemRequestSize = item.DesiredSize;
                    offsetX[minIndex] += itemRequestSize.Width;
                }
                requestSize.Width = offsetX.Max();
            }

            //返回我们面板需要的大小
            return requestSize;

        }

        #endregion
        #region 排列
        protected override Size ArrangeOverride(Size finalSize)
        {
            //建立两个列表储存 Item 的X坐标和Y坐标
            List<Double> offsetX = new List<Double>();
            List<Double> offsetY = new List<Double>();

            //最短栈默认为第一个
            int minIndex = 0;

            //判定布局类型
            if (Orientation == Orientation.Vertical)
            {
                //纵向布局

                //初始化坐标,由于是纵向布局,纵坐标是从0开始,横坐标则是固定值
                for (int i = 0; i < StatckCount; i++)
                {
                    //这里的GetOffsetX是计算每个栈的横坐标,计算过程是这样的:
                    //(int index) => index * (this.DesiredSize.Width + StatckSpacing) / StatckCount
                    offsetX.Add(GetOffsetX(i));
                    offsetY.Add(0);
                }

                //遍历 Children 进行布局
                foreach (var item in this.Children)
                {
                    //取最短的 Stack 加入新的 Item
                    double min = offsetY.Min();
                    //获取最短的 Stack 的编号
                    minIndex = offsetY.IndexOf(min);

                    //对 item 进行布局
                    item.Arrange(new Rect(offsetX[minIndex], offsetY[minIndex], item.DesiredSize.Width, item.DesiredSize.Height));
                    //递增纵坐标
                    offsetY[minIndex] += (item.DesiredSize.Height + ItemsSpacing);
                }
            }
            else
            {
                //横向布局,内容也是大同小异

                for (int i = 0; i < StatckCount; i++)
                {
                    offsetX.Add(0);
                    //这里的 GetOffsetY 的计算过程如下:
                    //(int index) => index * (this.DesiredSize.Height + StatckSpacing) / StatckCount
                    offsetY.Add(GetOffsetY(i));
                }

                foreach (var item in this.Children)
                {
                    double min = offsetX.Min();
                    minIndex = offsetX.IndexOf(min);

                    item.Arrange(new Rect(offsetX[minIndex], offsetY[minIndex], item.DesiredSize.Width, item.DesiredSize.Height));
                    offsetX[minIndex] += (item.DesiredSize.Width + ItemsSpacing);
                }
            }

            //直接返回参数
            return finalSize;

        }

        private double GetOffsetX(int i)
        {
            var index = i * (this.DesiredSize.Width + ItemsSpacing) / StatckCount;
            return index;            
        }

        private double GetOffsetY(int i)
        {
            var index = i * (this.DesiredSize.Height + ItemsSpacing) / StatckCount;
            return index;
        }

        #endregion
    }
}
