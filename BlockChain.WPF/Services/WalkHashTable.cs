﻿using System;
using System.Threading.Tasks;
using BlockChain.Extensions;
using BlockChain.WPF.Messaging;
using Info.Blockchain.API;

namespace BlockChain.WPF.Services {

    public class WalkHashTable{

        public WalkHashTable(MessageCollection messages){
            _messages = messages;
            _api = new BlockchainApiHelper();
        }

        readonly MessageCollection _messages;
        readonly BlockchainApiHelper _api;

        public async Task Search(string txId){

            txId = txId.Trim(' ', '\r', '\n');

            _messages.AddHeading("Finding Transactions in Wikileaks Hash Trailer");

            try{
                await SearchTransaction(txId);
            }
            catch (Exception ex){
                _messages.Add(ex.Message, MessageType.Error);
            }

            _messages.AddCompletion();
        }

        async Task SearchTransaction(string txId){

            if (_messages.Cancel)
                return;

            var transaction = await _api.BlockExpolorer.GetTransactionAsync(txId);

            if (transaction.Outputs.Count > 2){
                _messages.Add($"The number of outs was more than 2");
                return;
            }
            
            if (transaction.Inputs.Count > 1) {
                _messages.Add($"The number of inputs was more than 1");
            }
            
            foreach (var txOut in transaction.Outputs) {

                if (txOut.Spent == false) {
                    _messages.Add("");
                    _messages.Add($"{transaction.Hash}");
                    _messages.Add($"{txOut.Script.ToInnerBytes()}");
                    _messages.Add($"{transaction.Time} - {txOut.Value}");
                }
            }

            foreach (var txOut in transaction.Outputs) {

                if (txOut.Spent){

                    var address = await _api.BlockExpolorer.GetAddressAsync(txOut.Address);

                    foreach (var txIn in address.Transactions){
                        if (txIn.Hash != transaction.Hash){
                            await SearchTransaction(txIn.Hash);
                        }
                    }

                }
            }

        }
    }
}
