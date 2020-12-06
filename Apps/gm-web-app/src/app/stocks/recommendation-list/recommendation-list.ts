import { Sort } from '@angular/material/sort';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

export class RecommendationList {
    private _recommendations!: Array<StockRecommendation>;
    get recommendations(): Array<StockRecommendation> {
        return this._recommendations;
    }
    set recommendations(value: Array<StockRecommendation>) {
        this._recommendations = value;
        this._filtered = value.slice();
        this._sorted = value.slice();
    }

    private _sorted!: Array<StockRecommendation>;
    get sorted(): Array<StockRecommendation> {
        return this._sorted;
    }

    private _filtered!: Array<StockRecommendation>;
    get filtered(): Array<StockRecommendation> {
        return this._filtered;
    }

    constructor() {
        this._symbolFilter = '';
        this._recommendations = new Array<StockRecommendation>();
        this._filtered = new Array<StockRecommendation>();
        this._sorted = new Array<StockRecommendation>();
    }

    public contains(symbol: string): boolean {
      if(this._recommendations.find(x => x.symbol.toLowerCase() === symbol.toLowerCase())) {
        return true;
      }
      return false;
    }

    public add(r: StockRecommendation) {
      if(this._recommendations.find(x => x.symbol.toLowerCase() === r.symbol.toLowerCase())) {
        return;
      }

      this._recommendations.push(r);
      this._sorted.push(r);
      this._filtered.push(r);
    }

    public remove(symbol: string) {
      let index = this._recommendations.findIndex(x => x.symbol.toLowerCase() === symbol.toLowerCase());
      if(index < 0) {
        return;
      }
      this._recommendations.splice(index, 1);
      index = this._sorted.findIndex(x => x.symbol.toLowerCase() === symbol.toLowerCase());
      if(index < 0) {
        return;
      }
      this._sorted.splice(index, 1);

      index = this._filtered.findIndex(x => x.symbol.toLowerCase() === symbol.toLowerCase());
      if(index < 0) {
        return;
      }
      this._filtered.splice(index, 1);
    }

    public clear() {
      this._recommendations = new Array<StockRecommendation>();
      this._filtered = new Array<StockRecommendation>();
      this._sorted = new Array<StockRecommendation>();
    }

    public includes(symbol: string): boolean {
      return this._recommendations.findIndex(x => x.symbol.toLowerCase() === symbol.toLowerCase()) >= 0;
    }

    private _symbolFilter: string;
    public applyFilter(symbol: string) {
        this._symbolFilter = symbol.toLocaleLowerCase(); 
        if(this._symbolFilter !== '') {
          this._filtered = this.recommendations.filter(x => x.symbol.toLocaleLowerCase().includes(this._symbolFilter));
        } else {
          this._filtered = this.recommendations;
        }
        this._sorted = this._filtered.slice();
    }

    public applySort(sort: Sort) {
        const data = this._filtered.slice();
        if (!sort.active || sort.direction === '') {
          this._sorted = data;
          return;
        }
    
        this._sorted = data.sort((a, b) => {
          const isAsc = sort.direction === 'asc';
          if(sort.active === 'symbol') {
            return this.compare(a.symbol, b.symbol, isAsc);
          } else if(sort.active === 'sentiment'){
            return this.compare(a.sentiment, b.sentiment, isAsc);
          } else if(sort.active === 'prediction'){
            return this.compare(a.prediction, b.prediction, isAsc);
          } else if(sort.active === 'lowPrediction'){
            return this.compare(a.lowPrediction, b.lowPrediction, isAsc);
          } else {
            return 0;
          }
        });
    }

    private compare(a: number | string, b: number | string, isAsc: boolean) {
        return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
    }

}
