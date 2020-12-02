import { Component, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationService } from '../stock-recommendation.service';

@Component({
  selector: 'gm-futures',
  templateUrl: './futures.component.html',
  styleUrls: ['./futures.component.scss']
})
export class FuturesComponent implements OnInit {

  public recommendations: Array<StockRecommendation>;
  public selectedFuture?: StockRecommendation;
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.recommendations = new Array<StockRecommendation>();
   }

  ngOnInit(): void {
    this.stockRecommendationService.getFutures().subscribe(x => {
      for(let r of x) {
        this.recommendations.push(r);
      }
      
      if(this.recommendations.length > 0) {
        this.selectedFuture = this.recommendations[0];
      }
    })
  }

  selectFuture(r: StockRecommendation) {
    if(this.selectedFuture && r.symbol == this.selectedFuture.symbol) {
      this.selectedFuture = undefined;
    } else {
      this.selectedFuture = r;
    }
  }
}
