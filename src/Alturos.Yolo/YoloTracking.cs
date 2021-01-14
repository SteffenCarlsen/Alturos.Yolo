﻿#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Alturos.Yolo.Model;

#endregion

namespace Alturos.Yolo
{
    public class YoloTracking
    {
        private readonly int _frameHeight;
        private readonly int _frameWidth;
        private readonly Dictionary<string, YoloTrackingItemExtended> _trackingItems;
        private int _nextObjectId;

        private int _processIndex;

        public YoloTracking(int frameWidth, int frameHeight)
        {
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _trackingItems = new Dictionary<string, YoloTrackingItemExtended>();
        }

        public void Reset()
        {
            _processIndex = 0;
            _trackingItems.Clear();
        }

        public IEnumerable<YoloTrackingItem> Analyse(IEnumerable<YoloItem> items)
        {
            _processIndex++;

            if (_trackingItems.Count == 0)
            {
                foreach (var item in items)
                {
                    var trackingItem = new YoloTrackingItemExtended(item, GetObjectId());
                    _trackingItems.Add(trackingItem.ObjectId, trackingItem);
                }

                return new YoloTrackingItem[0];
            }

            var trackingItems = new List<YoloTrackingItem>();

            foreach (var item in items)
            {
                var bestMatch = _trackingItems.Values.Select(o => new
                    {
                        Item = o,
                        DistancePercentage = DistancePercentage(o.Center(), item.Center()),
                        SizeDifference = GetSizeDifferencePercentage(o, item)
                    })
                    .Where(o => !trackingItems.Select(x => x.ObjectId).Contains(o.Item.ObjectId) &&
                                o.DistancePercentage <= 15 && o.SizeDifference < 30)
                    .OrderBy(o => o.DistancePercentage)
                    .FirstOrDefault();

                if (bestMatch == null || bestMatch.Item.ProcessIndex + 25 < _processIndex)
                {
                    var trackingItem1 = new YoloTrackingItemExtended(item, GetObjectId())
                    {
                        ProcessIndex = _processIndex
                    };

                    _trackingItems.Add(trackingItem1.ObjectId, trackingItem1);
                    continue;
                }

                bestMatch.Item.X = item.X;
                bestMatch.Item.Y = item.Y;
                bestMatch.Item.Width = item.Width;
                bestMatch.Item.Height = item.Height;
                bestMatch.Item.ProcessIndex = _processIndex;
                bestMatch.Item.IncreaseTrackingConfidence();

                if (bestMatch.Item.TrackingConfidence >= 60)
                {
                    var trackingItem = new YoloTrackingItem(item, bestMatch.Item.ObjectId);
                    trackingItems.Add(trackingItem);
                }
            }

            var itemsWithoutHits = _trackingItems.Values.Where(o => o.ProcessIndex != _processIndex);
            foreach (var item in itemsWithoutHits)
            {
                item.DecreaseTrackingConfidence();
            }

            return trackingItems;
        }

        private string GetObjectId()
        {
            _nextObjectId++;
            return $"O{_nextObjectId:00000}";
        }

        private double GetSizeDifferencePercentage(YoloTrackingItemExtended item1, YoloItem item2)
        {
            var area1 = item1.Width * item1.Height;
            var area2 = item2.Width * item2.Height;

            if (area1 == area2)
            {
                return 0;
            }

            if (area1 > area2)
            {
                var change1 = 100.0 * area2 / area1;
                return 100 - change1;
            }

            var change = 100.0 * area1 / area2;
            return 100 - change;
        }

        private double DistancePercentage(Point p1, Point p2)
        {
            var max = Distance(new Point(0, 0), new Point(_frameWidth, _frameHeight));
            var current = Distance(p1, p2);

            return 100.0 * current / max;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Pow2(p2.X - p1.X) + Pow2(p2.Y - p1.Y));
        }

        private double Pow2(double x)
        {
            return x * x;
        }
    }
}