import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { AuthenticationService } from '../users/authentication.service';
import { StockData } from './stock-data';
import { StockRecommendation } from './stock-recommendation';
import { StockRecommendationHistory } from './stock-recommendation-history';

@Injectable({
  providedIn: 'root'
})
export class StockRecommendationService {
  constructor(private http: HttpClient, private authenticationService: AuthenticationService) { }

  private url = environment.apiUrl + '/recommendations';

  public getDailyPicks(): Observable<Array<StockRecommendation>> {
    return this.http.get<Array<StockRecommendation>>(this.url + '/stocks/daily')
    .pipe(map(stocks => {
      let mappedDailies = new Array<StockRecommendation>();
      for(let f of stocks) {
        mappedDailies.push(new StockRecommendation(f.date, f.symbol, f.systemId,
          f.sentiment, f.confidence, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget, 
          new StockData(f.lastData.date, f.lastData.symbol, 
            f.lastData.open, f.lastData.high, f.lastData.low, f.lastData.close, 
            f.lastData.adjustedClose, f.lastData.volume, f.lastData.previousClose)));
      }
      return mappedDailies;
    }));
  }

  public getFutures(): Observable<Array<StockRecommendation>> {
    return this.http.get<Array<StockRecommendation>>(this.url + '/futures')
    .pipe(map(futures => {
      let mappedFutures = new Array<StockRecommendation>();
      for(let f of futures) {
        mappedFutures.push(new StockRecommendation(f.date, f.symbol, f.systemId,
          f.sentiment, f.confidence, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget, 
          new StockData(f.lastData.date, f.lastData.symbol, 
            f.lastData.open, f.lastData.high, f.lastData.low, f.lastData.close, 
            f.lastData.adjustedClose, f.lastData.volume, f.lastData.previousClose)));
      }
      return mappedFutures;
    }));
  }

  public getRecommendationFor(symbol: string): Observable<StockRecommendation> {
    return this.http.get<StockRecommendation>(this.url + '/stocks/' + symbol)
    .pipe(map(r => {
      return new StockRecommendation(r.date, r.symbol, r.systemId,
        r.sentiment, r.confidence, r.prediction, r.lowPrediction, r.previousClose,
        r.predictedPriceTarget, r.predictedLowTarget, 
        new StockData(r.lastData.date, r.lastData.symbol, 
          r.lastData.open, r.lastData.high, r.lastData.low, r.lastData.close, 
          r.lastData.adjustedClose, r.lastData.volume, r.lastData.previousClose));
    }));
  }

  public getHistoryFor(symbol: string): Observable<StockRecommendationHistory> {
    return this.http.get<StockRecommendationHistory>(this.url + '/stocks/history/' + symbol)
    .pipe(map(h => {
      let recommendations = new Array<StockRecommendation>();
      for(let r of h.historicalData) {
        recommendations.push(this.clone(r));
      }
      let mapped = new StockRecommendationHistory(h.systemId, h.symbol, h.lastUpdated, 
          this.clone(h.lastRecommendation), recommendations);
      return mapped;
    }));
  }

  private clone(r :StockRecommendation) : StockRecommendation {
    return new StockRecommendation(r.date, r.symbol, r.systemId,
      r.sentiment, r.confidence, r.prediction,
      r.lowPrediction, r.previousClose,
      r.predictedPriceTarget, r.predictedLowTarget,
      new StockData(r.lastData.date, r.lastData.symbol,
        r.lastData.open, r.lastData.high, r.lastData.low, r.lastData.close,
        r.lastData.adjustedClose, r.lastData.volume, r.lastData.previousClose))
  }

  public getUserWatchlistRecommendations(): Observable<Array<StockRecommendation>> {
    const username = this.authenticationService.currentUserValue.username;
    return this.http.get<Array<StockRecommendation>>(this.url + '/stocks/user/' + username)
    .pipe(map(picks => {
      let mapped = new Array<StockRecommendation>();
      for(let f of picks) {
        mapped.push(new StockRecommendation(f.date, f.symbol, f.systemId,
          f.sentiment, f.confidence, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget, 
          new StockData(f.lastData.date, f.lastData.symbol, 
            f.lastData.open, f.lastData.high, f.lastData.low, f.lastData.close, 
            f.lastData.adjustedClose, f.lastData.volume, f.lastData.previousClose)));
      }
      return mapped;
    }));
  }
}
