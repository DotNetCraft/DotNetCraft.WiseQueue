using System;
using DotNetCraft.Common.Core.Utils.Logging;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Converters;
using Newtonsoft.Json;

namespace DotNetCraft.WiseQueue.Domain.Common.Converters
{
    /// <summary>
    /// Converting object into JSON and JSON into the object.
    /// </summary>
    public sealed class JsonConverter : IJsonConverter
    {
        private readonly ICommonLogger logger = LogManager.GetCurrentClassLogger();

        #region Fields...
        /// <summary>
        /// The <see cref="JsonSerializerSettings"/> instance.
        /// </summary>
        private readonly JsonSerializerSettings jsonSerializerSettings;
        #endregion

        #region Constructors...
        /// <summary>
        /// Constructor.
        /// </summary>
        public JsonConverter()
        {
            jsonSerializerSettings = new JsonSerializerSettings();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonSerializerSettings">The <see cref="JsonSerializerSettings"/> instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="jsonSerializerSettings"/> is <see langword="null" />.</exception>
        public JsonConverter(JsonSerializerSettings jsonSerializerSettings)
        {
            if (jsonSerializerSettings == null)
                throw new ArgumentNullException("jsonSerializerSettings");

            this.jsonSerializerSettings = jsonSerializerSettings;
        }
        #endregion

        #region Implementation of IJsonConverter

        /// <summary>
        /// <c>Convert</c> <c>object</c> into the JSON.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <returns>JSON.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null" />.</exception>
        public string ConvertToJson(object data)
        {
            logger.Trace("Converting {0} into JSON...", data);

            if (data == null)
                throw new ArgumentNullException("data");

            string result = JsonConvert.SerializeObject(data, jsonSerializerSettings);

            logger.Trace("The {0} has been converted into JSON: {1}", data, result);
            return result;
        }

        /// <summary>
        /// <c>Convert</c> JSON data into the <c>object</c>.
        /// </summary>
        /// <typeparam name="TObject"><see cref="Type"/> of object.</typeparam>
        /// <param name="jsonData">JSON.</param>
        /// <returns>The <typeparamref name="TObject"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="jsonData"/> is <see langword="null" />.</exception>
        public TObject ConvertFromJson<TObject>(string jsonData)
        {
            logger.Trace("Converting JSON ({0}) into object (Type: {1})...", jsonData, typeof(TObject));

            if (string.IsNullOrWhiteSpace(jsonData))
                throw new ArgumentNullException("jsonData");

            TObject result = JsonConvert.DeserializeObject<TObject>(jsonData, jsonSerializerSettings);

            logger.Trace("The JSON ({0}) has been converted into {1}", jsonData, result);
            return result;
        }

        /// <summary>
        /// <c>Convert</c> JSON data into the <c>object</c>.
        /// </summary>
        /// <param name="jsonData">JSON.</param>
        /// <param name="type"><see cref="Type"/> of object</param>
        /// <returns>The object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="jsonData"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null" />.</exception>
        public object ConvertFromJson(string jsonData, Type type)
        {
            logger.Trace("Converting JSON ({0}) into object (Type: {1})...", jsonData, type);

            if (string.IsNullOrWhiteSpace(jsonData))
                throw new ArgumentNullException("jsonData");
            if (type == null)
                throw new ArgumentNullException("type");

            var result = JsonConvert.DeserializeObject(jsonData, type, jsonSerializerSettings);

            logger.Trace("The JSON ({0}) has been converted into {1}", jsonData, result);
            return result;
        }

        #endregion
    }

}
