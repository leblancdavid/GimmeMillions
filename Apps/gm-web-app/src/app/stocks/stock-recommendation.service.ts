import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { AuthenticationService } from '../users/authentication.service';
import { StockRecommendation } from './stock-recommendation';

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
          f.sentiment, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget));
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
          f.sentiment, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget));
      }
      return mappedFutures;
    }));
  }

  public getRecommendationFor(symbol: string): Observable<StockRecommendation> {
    return this.http.get<StockRecommendation>(this.url + '/stocks/' + symbol)
    .pipe(map(r => {
      return new StockRecommendation(r.date, r.symbol, r.systemId,
        r.sentiment, r.prediction, r.lowPrediction, r.previousClose,
        r.predictedPriceTarget, r.predictedLowTarget);
    }));
  }

  public getUserWatchlistRecommendations(): Observable<Array<StockRecommendation>> {
    const username = this.authenticationService.currentUserValue.username;
    return this.http.get<Array<StockRecommendation>>(this.url + '/stocks/user/' + username)
    .pipe(map(picks => {
      let mapped = new Array<StockRecommendation>();
      for(let f of picks) {
        mapped.push(new StockRecommendation(f.date, f.symbol, f.systemId,
          f.sentiment, f.prediction, f.lowPrediction, f.previousClose,
          f.predictedPriceTarget, f.predictedLowTarget));
      }
      return mapped;
    }));
  }
}
