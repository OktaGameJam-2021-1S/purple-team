const _=require("lodash");
const express=require("express");
const router=express.Router();

const accounts={
	"1": {
		id: "1",
		name: "#TeamRoxo_1",
		games: 1
	}
};
const scores=[
	{
		player1: "#TeamRoxo_1",
		player2: "#TeamRoxo_2",
		score: 100
	}
];

function _handshake(req, res) {
	return res.send({ status: "ok" })
	          .status(200);
}

function _login(req, res) {
	if(accounts[req.params.uid])
		return res.send(accounts[req.params.uid])
		          .status(200);

	accounts[req.params.uid]={
		id: req.params.uid,
		name: req.params.name,
		games: 0
	};
	return res.send(accounts[req.params.uid])
	          .status(201);
}

function _saveScore(req, res) {
	accounts[req.params.player1].games++;
	accounts[req.params.player2].games++;
	scores.push({
		            score: req.params.score,
		            player1: accounts[req.params.player1].name,
		            player2: accounts[req.params.player2].name
	            });
	return _top10(req, res);
}

function _top10(req, res) {
	const top10=_.orderBy(scores, "score", "desc")
	             .slice(0, 10);
	return res.send(top10)
	          .status(200);
}

router.get("/", _handshake);

router.get("/login/:uid/:name", _login);

router.get("/scores/save/:player1/:player2/:score", _saveScore);

router.get("/scores/top10", _top10);

module.exports=router;
