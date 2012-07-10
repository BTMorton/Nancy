﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Nancy.Responses.Negotiation
{
    public class XmlProcessor : IResponseProcessor
    {
        private readonly ISerializer serializer;

        public XmlProcessor(IEnumerable<ISerializer> serializers)
        {
            this.serializer = serializers.FirstOrDefault(x => x.CanSerialize("application/xml"));
        }

        private static IEnumerable<Tuple<string, MediaRange>> extensionMappings =
                            new[] { new Tuple<string, MediaRange>("xml", MediaRange.FromString("application/xml")) };

        /// <summary>
        /// Gets a set of mappings that map a given extension (such as .json)
        /// to a media range that can be sent to the client in a vary header.
        /// </summary>
        public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
        {
            get
            {
                return extensionMappings;
            }
        }

        /// <summary>
        /// Determines whether the the processor can handle a given content type and model
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A ProcessorMatch result that determines the priority of the processor</returns>
        public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            if (this.IsExactXmlContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.ExactMatch
                };
            }

            if (this.IsWildcardXmlContentType(requestedMediaRange))
            {
                return new ProcessorMatch
                {
                    ModelResult = MatchResult.DontCare,
                    RequestedContentTypeResult = MatchResult.NonExactMatch
                };
            }

            return new ProcessorMatch
            {
                ModelResult = MatchResult.DontCare,
                RequestedContentTypeResult = MatchResult.NoMatch
            };
        }

        /// <summary>
        /// Process the response
        /// </summary>
        /// <param name="requestedMediaRange">Content type requested by the client</param>
        /// <param name="model">The model for the given media range</param>
        /// <param name="context">The nancy context</param>
        /// <returns>A response</returns>
        public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
        {
            return new XmlResponse<object>(model, "application/xml", this.serializer);
        }

        private bool IsExactXmlContentType(MediaRange requestedContentType)
        {
            if (requestedContentType.Type.IsWildcard && requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            return requestedContentType.Matches("application/xml") || requestedContentType.Matches("text/xml");
        }

        private bool IsWildcardXmlContentType(MediaRange requestedContentType)
        {
            if (!requestedContentType.Type.IsWildcard && !string.Equals("application", requestedContentType.Type, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (requestedContentType.Subtype.IsWildcard)
            {
                return true;
            }

            var subtypeString = requestedContentType.Subtype.ToString();

            return (subtypeString.StartsWith("vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    subtypeString.EndsWith("+xml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}