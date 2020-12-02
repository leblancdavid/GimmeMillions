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
  constructor(private stockRecommendationService: StockRecommendationService) {
    this.recommendations = new Array<StockRecommendation>();
   }

  ngOnInit(): void {
    this.stockRecommendationService.getFutures().subscribe(x => {
      this.recommendations = x;
    })
  }

}
