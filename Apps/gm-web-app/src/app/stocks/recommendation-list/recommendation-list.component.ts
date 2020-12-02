import { Component, Input, OnInit } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { StockRecommendation } from '../stock-recommendation';

@Component({
  selector: 'gm-recommendation-list',
  templateUrl: './recommendation-list.component.html',
  styleUrls: ['./recommendation-list.component.scss']
})
export class RecommendationListComponent implements OnInit {

  @Input() recommendations!: Array<StockRecommendation>;
  public selectedItem?: StockRecommendation;
  public sortedRecommendations!: Array<StockRecommendation>;
  public symbolFilter: string;
  constructor() {
    this.symbolFilter = '';
  }

  ngOnInit(): void {
    this.sortedRecommendations = this.recommendations;
  }

  select(r: StockRecommendation) {
    if (this.selectedItem && r.symbol == this.selectedItem.symbol) {
      this.selectedItem = undefined;
    } else {
      this.selectedItem = r;
    }
  }

  sortRecommendations(sort: Sort) {
    const data = this.recommendations.slice();
    if (!sort.active || sort.direction === '') {
      this.sortedRecommendations = data;
      return;
    }

    this.sortedRecommendations = data.sort((a, b) => {
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

