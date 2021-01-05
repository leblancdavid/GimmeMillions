import { Component, OnInit } from '@angular/core';
import { RouteConfigLoadEnd } from '@angular/router';
import { RecommendationList } from '../recommendation-list/recommendation-list';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-futures',
  templateUrl: './futures.component.html',
  styleUrls: ['./futures.component.scss']
})
export class FuturesComponent implements OnInit {

  public futures: RecommendationList;
  public selectedFuture?: StockRecommendation;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.futures = new RecommendationList();
   }

  ngOnInit(): void {
   this.refresh(); 
  }

  refresh() {
    this.futures = new RecommendationList();
    this.selectedFuture = undefined;
    this.stockRecommendationService.getFutures().subscribe(x => {
      this.futures.recommendations = x;
    });
  }

  selectFuture(r: StockRecommendation) {
    if(this.selectedFuture && r.symbol == this.selectedFuture.symbol) {
      this.selectedFuture = undefined;
    } else {
      this.selectedFuture = r;
    }
  }
}
