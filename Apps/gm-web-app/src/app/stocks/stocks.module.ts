import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MaterialModule } from '../material/material.module';
import { FuturesComponent } from './futures/futures.component';
import { StocksComponent } from './stocks.component';
import { StockRecommendationItemComponent } from './stock-recommendation-item/stock-recommendation-item.component';
import { UserWatchlistComponent } from './user-watchlist/user-watchlist.component';
import { RecommendationListComponent } from './recommendation-list/recommendation-list.component';
import { StockSearchComponent } from './stock-search/stock-search.component';
import { StockDetailsComponent } from './stock-details/stock-details.component';
import { DailyPredictionsComponent } from './daily-predictions/daily-predictions.component';



@NgModule({
  declarations: [StocksComponent, FuturesComponent, StockRecommendationItemComponent, UserWatchlistComponent, RecommendationListComponent, StockSearchComponent, StockDetailsComponent, DailyPredictionsComponent],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule,
    MaterialModule
  ]
})
export class StocksModule { }
