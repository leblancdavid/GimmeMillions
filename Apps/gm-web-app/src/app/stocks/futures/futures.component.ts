import { Component, OnInit } from '@angular/core';
import { StockRecommendation } from '../stock-recommendation';

@Component({
  selector: 'gm-futures',
  templateUrl: './futures.component.html',
  styleUrls: ['./futures.component.scss']
})
export class FuturesComponent implements OnInit {

  public temporaryRecommendation: StockRecommendation
  constructor() {
    this.temporaryRecommendation = new StockRecommendation(new Date(),
      'DIA', 'Test System', 0.0, 
      4.2, -4.2, 42.0, 54.21, 35.76);
   }

  ngOnInit(): void {
  }

}
