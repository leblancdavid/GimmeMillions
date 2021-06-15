import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { MaterialModule } from '../material/material.module';
import { ChartsModule } from 'ng2-charts'
import { FuturesComponent } from './futures/futures.component';
import { StocksComponent } from './stocks.component';
import { StockRecommendationItemComponent } from './stock-recommendation-item/stock-recommendation-item.component';
import { UserWatchlistComponent } from './user-watchlist/user-watchlist.component';
import { RecommendationListComponent } from './recommendation-list/recommendation-list.component';
import { StockDetailsComponent } from './stock-details/stock-details.component';
import { DailyPredictionsComponent } from './daily-predictions/daily-predictions.component';
import { TrendIconComponent } from './stock-recommendation-item/trend-icon/trend-icon.component';
import { ConsensusPipe } from './stock-recommendation-item/consensus.pipe';
import { ResourcesModule } from '../resources/resources.module';
import { RecommendationHistoryDialogComponent } from './recommendation-history-dialog/recommendation-history-dialog.component';
import { HistoryChartComponent } from './history-chart/history-chart.component';
import { SentimentHistoryChartComponent } from './sentiment-history-chart/sentiment-history-chart.component';



@NgModule({
  declarations: [
    StocksComponent, 
    FuturesComponent, 
    StockRecommendationItemComponent, 
    UserWatchlistComponent, 
    RecommendationListComponent,
    StockDetailsComponent,
    DailyPredictionsComponent, 
    TrendIconComponent, 
    ConsensusPipe, 
    RecommendationHistoryDialogComponent, 
    HistoryChartComponent, 
    SentimentHistoryChartComponent
  ],
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule,
    MaterialModule,
    ResourcesModule,
    ChartsModule
  ],
  entryComponents: [
    RecommendationHistoryDialogComponent
  ],
})
export class StocksModule { }
