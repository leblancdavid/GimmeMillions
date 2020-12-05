import { EventEmitter, Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { AuthenticationService } from 'src/app/users/authentication.service';
import { RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Injectable({
  providedIn: 'root'
})
export class UserWatchlistService {

  constructor(private authenticationService: AuthenticationService, 
    private stockRecommendationService: StockRecommendationService) { 
    this._watchlist = new RecommendationList();
  }

  public watchlistUpdate = new EventEmitter<any>();

  private _watchlist: RecommendationList;
  public get watchlist(): RecommendationList {
    return this._watchlist;
  }

  public addToWatchlist(r: StockRecommendation) {
    this.authenticationService.addToWatchlist(r.symbol).subscribe(x => {
      this._watchlist.add(r);
      this.watchlistUpdate.emit();
    });
  }

  public removeFromWatchlist(symbol: string) {
    this.authenticationService.removeFromWatchlist(symbol).subscribe(x => {
      this._watchlist.remove(symbol);
      this.watchlistUpdate.emit();
    });
  }

  public refreshWatchlist() {
    this._watchlist.clear();
    return this.stockRecommendationService.getUserWatchlistRecommendations()
      .pipe(map(picks => {
        this._watchlist.recommendations = picks;
      }));
  }

  public includes(symbol: string): boolean {
    return this._watchlist.recommendations.findIndex(x => x.symbol.toLowerCase() === symbol.toLowerCase()) >= 0;
  }
}
