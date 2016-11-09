﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Spatial4n.Core.Context.Nts
{
    public class NtsSpatialContextFactory : SpatialContextFactory
    {
        protected static readonly PrecisionModel defaultPrecisionModel = new PrecisionModel();//floating

        //These 3 are JTS defaults for new GeometryFactory()
        public PrecisionModel precisionModel = defaultPrecisionModel;
        public int srid = 0;
        public ICoordinateSequenceFactory coordinateSequenceFactory = CoordinateArraySequenceFactory.Instance;

        //ignored if geo=false
        public NtsWktShapeParser.DatelineRule datelineRule = NtsWktShapeParser.DatelineRule.width180;

        public NtsWktShapeParser.ValidationRule validationRule = NtsWktShapeParser.ValidationRule.error;
        public bool autoIndex = false;
        public bool allowMultiOverlap = false;//ignored if geo=false

        //kinda advanced options:
        public bool useJtsPoint = true;
        public bool useJtsLineString = true;

        public NtsSpatialContextFactory()
        {
            base.wktShapeParserClass = typeof(NtsWktShapeParser);
            base.binaryCodecClass = typeof(NtsBinaryCodec);
        }

        protected override void Init(IDictionary<string, string> args/*, ClassLoader classLoader*/)
        {
            base.Init(args/*, classLoader*/);

            initField("datelineRule");
            initField("validationRule");
            initField("autoIndex");
            initField("allowMultiOverlap");
            initField("useJtsPoint");
            initField("useJtsLineString");

            String scaleStr = args["precisionScale"];
            String modelStr = args["precisionModel"];

            if (scaleStr != null)
            {
                if (modelStr != null && !modelStr.Equals("fixed"))
                    throw new ApplicationException("Since precisionScale was specified; precisionModel must be 'fixed' but got: " + modelStr);
                precisionModel = new PrecisionModel(double.Parse(scaleStr, CultureInfo.InvariantCulture));
            }
            else if (modelStr != null)
            {
                if (modelStr.Equals("floating"))
                {
                    precisionModel = new PrecisionModel(PrecisionModel.FLOATING);
                }
                else if (modelStr.Equals("floating_single"))
                {
                    precisionModel = new PrecisionModel(PrecisionModel.FLOATING_SINGLE);
                }
                else if (modelStr.Equals("fixed"))
                {
                    throw new ApplicationException("For fixed model, must specifiy 'precisionScale'");
                }
                else
                {
                    throw new ApplicationException("Unknown precisionModel: " + modelStr);
                }
            }
        }

        public virtual GeometryFactory GetGeometryFactory()
        {
            if (precisionModel == null || coordinateSequenceFactory == null)
                throw new InvalidOperationException("precision model or coord seq factory can't be null");
            return new GeometryFactory(precisionModel, srid, coordinateSequenceFactory);
        }

        protected override SpatialContext NewSpatialContext()
        {
            return new NtsSpatialContext(this);
        }
    }
}
