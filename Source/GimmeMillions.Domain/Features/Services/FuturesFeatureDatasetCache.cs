using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GimmeMillions.Domain.Features.Services
{
    public class FuturesFeatureDatasetCache
    {
        private IFeatureDatasetService<FeatureVector> _featureDatasetService;
        private Dictionary<DateTime, FeatureVector> _futuresCache;
        public FuturesFeatureDatasetCache(IFeatureDatasetService<FeatureVector> featureDatasetService)
        {
            _featureDatasetService = featureDatasetService;
            _futuresCache = new Dictionary<DateTime, FeatureVector>();
        }

        public Result<FeatureVector> GetFuturesFor(DateTime date)
        {
            if(_futuresCache.ContainsKey(date))
            {
                return _futuresCache[date];
            }

            var result = ComputeFutures(date);
            if(result.IsSuccess)
            {
                _futuresCache[date] = result.Value;
            }
            return result;
        }

        private Result<FeatureVector> ComputeFutures(DateTime date)
        {
            var dia = _featureDatasetService.GetFeatureVector("DIA", date);
            var spy = _featureDatasetService.GetFeatureVector("SPY", date);
            var qqq = _featureDatasetService.GetFeatureVector("QQQ", date);
            var rut = _featureDatasetService.GetFeatureVector("RUT", date);

            if(dia.IsFailure || spy.IsFailure || qqq.IsFailure || rut.IsFailure)
            {
                return Result.Failure<FeatureVector>("Unable to compute the Futures Vector");
            }

            var futures = new FeatureVector(
                dia.Value.Data
                .Concat(spy.Value.Data)
                .Concat(qqq.Value.Data)
                .Concat(rut.Value.Data).ToArray(), date,
                dia.Value.Encoding);
            return Result.Success(futures);
        }
    }
}
